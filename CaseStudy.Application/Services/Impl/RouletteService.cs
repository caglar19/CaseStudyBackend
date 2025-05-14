using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using CaseStudy.Application.Models.Roulette;
using CaseStudy.Application.Interfaces;
using CaseStudy.Application.Strategies;
using System.Text.RegularExpressions;

// Kullanılacak modelleri açıkça belirt
using RoulettePredictionResponseModel = CaseStudy.Application.Models.Roulette.RoulettePredictionResponse;
using RouletteInitializeResponseModel = CaseStudy.Application.Models.Roulette.RouletteInitializeResponse;
using RouletteExtractNumbersResponseModel = CaseStudy.Application.Models.Roulette.RouletteExtractNumbersResponse;

namespace CaseStudy.Application.Services.Impl
{
    public class RouletteService : IRouletteService
    {
        private readonly IMongoCollection<RouletteData> _rouletteCollection;
        private readonly IMongoCollection<PredictionResult> _predictionResultsCollection;
        private readonly IMongoCollection<PredictionRecord> _predictionRecordsCollection;
        private readonly string _defaultRouletteId = "default";
        
        // Strateji yöneticisi
        private readonly StrategyManager _strategyManager;
        
        // Son tahmin - gerçek sonuç karşılaştırması için
        private int _lastPredictedNumber = -1;

        public RouletteService(IOptions<MongoDBSettings> mongoSettings)
        {
            try
            {
                var client = new MongoClient(mongoSettings.Value.ConnectionString);
                var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
                _rouletteCollection = database.GetCollection<RouletteData>(mongoSettings.Value.RouletteCollectionName);
                _predictionResultsCollection = database.GetCollection<PredictionResult>(mongoSettings.Value.PredictionResultsCollectionName);
                _predictionRecordsCollection = database.GetCollection<PredictionRecord>(mongoSettings.Value.PredictionRecordsCollectionName);
                
                // Strateji yöneticisini başlat
                _strategyManager = new StrategyManager(mongoSettings);
            }
            catch
            {
                throw;
            }
        }

        private async Task<RouletteData> GetRouletteDataAsync()
        {
            return await _rouletteCollection.Find(r => r.Name == _defaultRouletteId).FirstOrDefaultAsync();
        }

        public async Task<RoulettePredictionResponseModel> InitializeNumbersAsync(List<int> initialNumbers)
        {
            try
            {
                if (initialNumbers == null || initialNumbers.Count == 0)
                {
                    return new RoulettePredictionResponseModel
                    {
                        Success = false,
                        Prediction = -1,
                        Numbers = new List<int>(),
                        ErrorMessage = "Geçerli rulet sayıları verilmedi."
                    };
                }

                var existingData = await GetRouletteDataAsync();

                if (existingData != null)
                {
                    // Mevcut veriyi güncelle
                    existingData.Numbers = initialNumbers;
                    await _rouletteCollection.ReplaceOneAsync(r => r.Name == _defaultRouletteId, existingData);
                }
                else
                {
                    // Yeni veri oluştur
                    var newData = new RouletteData
                    {
                        Name = _defaultRouletteId,
                        Numbers = initialNumbers
                    };
                    await _rouletteCollection.InsertOneAsync(newData);
                }

                // Tüm stratejilere tahmin yaptır ve en başarılı stratejinin tahmini döndür
                var (prediction, strategyName, topStrategies) = await _strategyManager.PredictNextNumberAsync(initialNumbers);
                
                // Son tahmin edilen sayıyı sakla (doğruluk takibi için)
                _lastPredictedNumber = prediction;
                
                return new RoulettePredictionResponseModel
                {
                    Success = true,
                    Prediction = prediction,
                    StrategyName = strategyName,
                    TopStrategies = topStrategies,
                    Numbers = initialNumbers
                };
            }
            catch (Exception ex)
            {
                return new RoulettePredictionResponseModel
                {
                    Success = false,
                    Prediction = -1,
                    Numbers = new List<int>(),
                    ErrorMessage = $"Rulet sayıları yüklenirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<RouletteInitializeResponseModel> InitializeWithNumbers(List<int> initialNumbers)
        {
            try
            {
                if (initialNumbers == null || initialNumbers.Count == 0)
                {
                    return new RouletteInitializeResponseModel
                    {
                        Success = false,
                        NumbersCount = 0
                    };
                }

                // InitializeNumbersAsync metodunu çağırarak MongoDB'ye kaydet
                await InitializeNumbersAsync(initialNumbers);
                var data = await GetRouletteDataAsync();
                int numbersCount = data?.Numbers?.Count ?? 0;
                
                return new RouletteInitializeResponseModel
                {
                    Success = true,
                    NumbersCount = numbersCount
                };
            }
            catch
            {
                return new RouletteInitializeResponseModel
                {
                    Success = false,
                    NumbersCount = 0
                };
            }
        }

        public async Task<RoulettePredictionResponseModel> AddNumberAndPredict(int number)
        {
            try
            {
                // Rulet verilerini al
                var rouletteData = await GetRouletteDataAsync();
                
                if (rouletteData == null)
                {
                    return new RoulettePredictionResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Rulet verileri bulunamadı. Önce initialize endpoint'ini çağırın."
                    };
                }
                
                // Yeni sayıyı ekle
                rouletteData.Numbers.Insert(0, number); // En başa ekle (en son eklenen)
                
                // Veritabanını güncelle
                await _rouletteCollection.ReplaceOneAsync(r => r.Name == _defaultRouletteId, rouletteData);
                
                // StrategyManager kullanarak yeni gelen sayıyla bir önceki tahmin için doğruluk güncellemesi yap
                await _strategyManager.UpdatePredictionAccuracyAsync(number);
                
                // StrategyManager kullanarak en başarılı strateji ile tahmin yap
                // PredictNextNumberAsync metodu artık en başarılı stratejiyi, strateji adını ve en iyi 3 stratejiyi döndürüyor
                var result = await _strategyManager.PredictNextNumberAsync(rouletteData.Numbers);
                int prediction = result.prediction;
                string strategyName = result.strategyName;
                var topStrategies = result.topStrategies;
                
                // Son tahmin edilen sayıyı sakla (doğruluk takibi için)
                _lastPredictedNumber = prediction;
                
                return new RoulettePredictionResponseModel
                {
                    Success = true,
                    Prediction = prediction,
                    StrategyName = strategyName,
                    TopStrategies = topStrategies,
                    Numbers = rouletteData.Numbers
                };
            }
            catch (Exception ex)
            {
                return new RoulettePredictionResponseModel
                {
                    Success = false,
                    ErrorMessage = $"Tahmin yapılırken hata oluştu: {ex.Message}"
                };
            }
        }

        public Task<RouletteExtractNumbersResponseModel> ExtractNumbersFromHtml(string htmlContent)
        {
            try
            {
                if (string.IsNullOrEmpty(htmlContent))
                {
                    return Task.FromResult(new RouletteExtractNumbersResponseModel
                    {
                        Success = false,
                        ErrorMessage = "HTML içeriği boş veya geçersiz",
                        Numbers = new List<int>(),
                        NumbersCount = 0
                    });
                }
                
                // HTML içeriğinden rulet sayılarını çıkar
                var numbers = new List<int>();
                
                // data-role="number-X" şeklindeki değerleri bul
                // Örnek: data-role="number-16"
                var regex = new Regex(@"data-role=""number-([0-9]+)""");
                var matches = regex.Matches(htmlContent);
                
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1 && int.TryParse(match.Groups[1].Value, out int number))
                    {
                        numbers.Add(number);
                    }
                }
                
                // Alternatif olarak <span class="value--dd5c7">X</span> şeklindeki değerleri de kontrol et
                var valueRegex = new Regex(@"<span class=""value--[a-z0-9]+"">(\d+)</span>");
                var valueMatches = valueRegex.Matches(htmlContent);
                
                foreach (Match match in valueMatches)
                {
                    if (match.Groups.Count > 1 && int.TryParse(match.Groups[1].Value, out int number))
                    {
                        // Eğer sayı daha önce eklenmemişse ekle
                        if (!numbers.Contains(number))
                        {
                            numbers.Add(number);
                        }
                    }
                }
                
                if (numbers.Count == 0)
                {
                    return Task.FromResult(new RouletteExtractNumbersResponseModel
                    {
                        Success = false,
                        ErrorMessage = "HTML içeriğinde rulet sayısı bulunamadı",
                        Numbers = new List<int>(),
                        NumbersCount = 0
                    });
                }
                
                return Task.FromResult(new RouletteExtractNumbersResponseModel
                {
                    Success = true,
                    Numbers = numbers,
                    NumbersCount = numbers.Count
                });
            }
            catch
            {
                return Task.FromResult(new RouletteExtractNumbersResponseModel
                {
                    Success = false,
                    ErrorMessage = "HTML içeriğinden rulet sayıları çıkarılırken hata oluştu",
                    Numbers = new List<int>(),
                    NumbersCount = 0
                });
            }
        }
    }
}
