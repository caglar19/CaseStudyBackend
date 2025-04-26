using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using CaseStudy.Application.Models.Roulette;
using CaseStudy.Application.Interfaces;
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
        private readonly IMongoCollection<StrategyPerformance> _strategyPerformanceCollection;
        private readonly IMongoCollection<PredictionRecord> _predictionRecordsCollection;
        private readonly string _defaultRouletteId = "default";
        
        // Rulet çarkındaki sayıların fiziksel dizilimi (saat yönünde)
        private readonly int[] _wheelSequence = new int[] {
            0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10, 5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
        };
        
        // Tahmin stratejilerinin adları
        private readonly List<string> _strategyNames = new List<string>
        {
            "hot_numbers",
            "cold_numbers",
            "odd_even_distribution",
            "high_low_distribution",
            "red_black_distribution",
            "sequence_analysis",
            "recurrence_intervals",
            "recent_numbers_penalty"
        };
        
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
                _strategyPerformanceCollection = database.GetCollection<StrategyPerformance>(mongoSettings.Value.StrategyPerformanceCollectionName);
                _predictionRecordsCollection = database.GetCollection<PredictionRecord>(mongoSettings.Value.PredictionRecordsCollectionName);
                
                // Strateji performans kayıtlarını kontrol et ve yoksa oluştur
                InitializeStrategyPerformanceTracking().Wait();
            }
            catch
            {
                throw;
            }
        }
        
        /// <summary>
        /// Strateji performans kayıtlarını kontrol eder ve yoksa oluşturur
        /// </summary>
        private async Task InitializeStrategyPerformanceTracking()
        {
            try
            {
                foreach (var strategyName in _strategyNames)
                {
                    var strategy = await _strategyPerformanceCollection.Find(s => s.StrategyName == strategyName).FirstOrDefaultAsync();
                    
                    if (strategy == null)
                    {
                        // Yeni strateji kaydı oluştur
                        await _strategyPerformanceCollection.InsertOneAsync(new StrategyPerformance
                        {
                            StrategyName = strategyName,
                            UsageCount = 0,
                            CorrectPredictionCount = 0,
                            DynamicWeight = 50, // Başlangıçta eşit ağırlık
                            LastUpdated = DateTime.Now,
                            RecentResults = new List<bool>()
                        });
                    }
                }
            }
            catch (Exception)
            {
                // Log hata - opsiyonel
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

                // Tahmin yap
                int prediction = PredictNextNumber(initialNumbers);
                
                // Son tahmin edilen sayıyı sakla (doğruluk takibi için)
                _lastPredictedNumber = prediction;
                
                // Tahminin 9-sağ/9-sol komşularını hesapla ve kaydet
                var neighbors = CalculateNeighbors(prediction);
                
                // Tahmin kaydını oluştur ve veritabanına kaydet
                var predictionRecord = new PredictionRecord
                {
                    PredictionDate = DateTime.UtcNow,
                    PredictedNumber = prediction,
                    ActualNumber = null, // Henüz bilinmiyor, gerçek sayı girildiğinde güncellenecek
                    IsCorrect = null, // Henüz bilinmiyor
                    Context = initialNumbers.Take(5).ToArray(), // Son 5 sayı (bağlam)
                    Strategy = "MultiTimeScaleAnalysis",
                    Neighbors = neighbors
                };
                
                await _predictionRecordsCollection.InsertOneAsync(predictionRecord);
                
                return new RoulettePredictionResponseModel
                {
                    Success = true,
                    Prediction = prediction,
                    Numbers = initialNumbers
                };
            }
            catch
            {
                return new RoulettePredictionResponseModel
                {
                    Success = false,
                    Prediction = -1,
                    Numbers = new List<int>(),
                    ErrorMessage = "Rulet sayıları yüklenirken hata oluştu."
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

        public async Task<RoulettePredictionResponseModel> AddNumberAndPredict(int newNumber)
        {
            try
            {
                var rouletteData = await GetRouletteDataAsync();
                
                if (rouletteData == null)
                {
                    return new RoulettePredictionResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Henüz başlangıç verileri yüklenmemiş. Önce InitializeNumbersAsync metodunu çağırın.",
                        Prediction = -1
                    };
                }
                
                // Yeni sayıyı ekle
                rouletteData.Numbers.Insert(0, newNumber); // Yeni sayıyı listenin başına ekle
                
                // Veritabanını güncelle
                await _rouletteCollection.ReplaceOneAsync(r => r.Name == _defaultRouletteId, rouletteData);
                
                // Tahmin yap
                int prediction = PredictNextNumber(rouletteData.Numbers);
                
                // Son tahmin edilen sayıyı sakla (doğruluk takibi için)
                _lastPredictedNumber = prediction;
                
                // Tahminin 9-sağ/9-sol komşularını hesapla ve kaydet
                var neighbors = CalculateNeighbors(prediction);
                
                // Tahmin kaydını oluştur ve veritabanına kaydet
                var predictionRecord = new PredictionRecord
                {
                    PredictionDate = DateTime.UtcNow,
                    PredictedNumber = prediction,
                    ActualNumber = null, // Henüz bilinmiyor, gerçek sayı girildiğinde güncellenecek
                    IsCorrect = null, // Henüz bilinmiyor
                    Context = rouletteData.Numbers.Take(5).ToArray(), // Son 5 sayı (bağlam)
                    Strategy = "MultiTimeScaleAnalysis",
                    Neighbors = neighbors
                };
                
                await _predictionRecordsCollection.InsertOneAsync(predictionRecord);
                
                return new RoulettePredictionResponseModel
                {
                    Success = true,
                    Prediction = prediction,
                    Numbers = rouletteData.Numbers
                };
            }
            catch (Exception ex)
            {
                return new RoulettePredictionResponseModel
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Prediction = -1
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

        #region Yardımcı Metodlar

        private int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count == 0)
            {
                return -1;
            }

            // Son sayıların tarih/saatleri gibi ek bilgileri olsaydı, bunları kullanarak 
            // zaman bazlı örüntüleri de analiz edebilirdik.
            // Şimdilik elimizdeki veri tabanını kullanacağız

            // 1. GELİŞTİRME: Sıcak sayılar stratejisi - frekans analizi
            // Sadece sık görülen sayılar değil, son zamanlarda yükselen sayıları da tespit edelim
            var allNumbers = numbers.GroupBy(n => n).ToDictionary(g => g.Key, g => g.Count());
            
            // Son 30, 60, 90 dönemdeki frekansları karşılaştır
            var last30 = numbers.Take(Math.Min(30, numbers.Count)).GroupBy(n => n).ToDictionary(g => g.Key, g => g.Count());
            var last60 = numbers.Take(Math.Min(60, numbers.Count)).GroupBy(n => n).ToDictionary(g => g.Key, g => g.Count());
            var last90 = numbers.Take(Math.Min(90, numbers.Count)).GroupBy(n => n).ToDictionary(g => g.Key, g => g.Count());
            
            // Trendi artan sayıları bul (son 30'da daha sık görülen sayılar)
            var trendingNumbers = new Dictionary<int, double>();
            foreach (var num in Enumerable.Range(0, 37))
            {
                double trend = 0;
                // Son 30'daki frekans daha yüksekse pozitif trend
                if (last30.ContainsKey(num))
                {
                    trend += 3.0 * last30[num] / Math.Min(30, numbers.Count);
                }
                if (last60.ContainsKey(num))
                {
                    trend += 2.0 * last60[num] / Math.Min(60, numbers.Count);
                }
                if (last90.ContainsKey(num))
                {
                    trend += 1.0 * last90[num] / Math.Min(90, numbers.Count);
                }
                
                if (trend > 0)
                {
                    trendingNumbers[num] = trend;
                }
            }
            
            // En yüksek trende sahip sayıları al
            var hotNumbers = trendingNumbers
                .OrderByDescending(kvp => kvp.Value)
                .Take(7)
                .Select(kvp => kvp.Key)
                .ToList();

            // 2. Soğuk sayılar stratejisi: En az tekrar eden sayıları bul
            var allPossibleNumbers = Enumerable.Range(0, 37).ToList(); // 0-36 arası rulet sayıları
            var coldNumbers = allPossibleNumbers
                .Except(numbers)
                .ToList();

            if (coldNumbers.Count == 0)
            {
                coldNumbers = allPossibleNumbers
                    .GroupBy(n => numbers.Count(pn => pn == n))
                    .OrderBy(g => g.Key)
                    .FirstOrDefault()?.ToList() ?? new List<int>();
            }

            // 3. GELİŞTİRME: Markov Zinciri benzeri bir yaklaşım
            // Her sayıdan sonra hangi sayının geldiğini analiz ederek geçiş matrisini oluştur
            var transitionMatrix = new Dictionary<int, Dictionary<int, int>>();
            
            // Tüm sayılar için geçiş matrisini başlat
            for (int i = 0; i <= 36; i++)
            {
                transitionMatrix[i] = new Dictionary<int, int>();
            }
            
            // Geçiş frekanslarını say
            for (int i = 0; i < numbers.Count - 1; i++)
            {
                int currentNum = numbers[i];
                int nextNum = numbers[i + 1];
                
                if (!transitionMatrix[currentNum].ContainsKey(nextNum))
                {
                    transitionMatrix[currentNum][nextNum] = 0;
                }
                
                transitionMatrix[currentNum][nextNum]++;
            }
            
            // Son birkaç sayıyı kontrol et ve mümkün olan sonraki sayıları bul
            var lastNumbers = numbers.Take(10).ToList();
            var markovCandidates = new Dictionary<int, double>();
            
            // Markov analizi - son sayıdan sonra en sık gelen sayılar
            if (lastNumbers.Count > 0)
            {
                int lastNum = lastNumbers[0];
                
                if (transitionMatrix.ContainsKey(lastNum))
                {
                    foreach (var kvp in transitionMatrix[lastNum].OrderByDescending(k => k.Value))
                    {
                        markovCandidates[kvp.Key] = kvp.Value;
                    }
                }
            }
            
            // Ayrıca son 2 sayı çiftinin en sık gelen 3. sayısını analiz et
            if (lastNumbers.Count >= 2)
            {
                var pairTransitions = new Dictionary<(int, int), Dictionary<int, int>>();
                
                // Çiftlerden sonraki sayıları hesapla
                for (int i = 0; i < numbers.Count - 2; i++)
                {
                    var pair = (numbers[i], numbers[i + 1]);
                    int nextNum = numbers[i + 2];
                    
                    if (!pairTransitions.ContainsKey(pair))
                    {
                        pairTransitions[pair] = new Dictionary<int, int>();
                    }
                    
                    if (!pairTransitions[pair].ContainsKey(nextNum))
                    {
                        pairTransitions[pair][nextNum] = 0;
                    }
                    
                    pairTransitions[pair][nextNum]++;
                }
                
                // Son 2 sayı çiftini kontrol et
                var lastPair = (lastNumbers[1], lastNumbers[0]);
                
                if (pairTransitions.ContainsKey(lastPair))
                {
                    foreach (var kvp in pairTransitions[lastPair].OrderByDescending(k => k.Value))
                    {
                        // Çift analizine daha yüksek ağırlık ver
                        if (markovCandidates.ContainsKey(kvp.Key))
                        {
                            markovCandidates[kvp.Key] += kvp.Value * 1.5;
                        }
                        else
                        {
                            markovCandidates[kvp.Key] = kvp.Value * 1.5;
                        }
                    }
                }
            }
            
            // 4. GELİŞTİRME: Sektör bazlı analiz ve gelişmiş dağılım analizi
            // Rulet çarkındaki komşu sayıları sektör olarak grupla
            var sectors = new List<List<int>>
            {
                new List<int> { 0, 32, 15, 19, 4, 21, 2, 25 },      // Sektör 1
                new List<int> { 17, 34, 6, 27, 13, 36, 11, 30 },     // Sektör 2
                new List<int> { 8, 23, 10, 5, 24, 16, 33, 1 },       // Sektör 3
                new List<int> { 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26 }  // Sektör 4
            };
            
            // Her sektörün son 50 sayıdaki frekansını hesapla
            var recentNumbers = numbers.Take(Math.Min(50, numbers.Count)).ToList();
            var sectorFrequency = new Dictionary<int, int>();
            
            for (int sectorIndex = 0; sectorIndex < sectors.Count; sectorIndex++)
            {
                sectorFrequency[sectorIndex] = recentNumbers.Count(n => sectors[sectorIndex].Contains(n));
            }
            
            // En az çıkan sektörü belirle
            int leastFrequentSectorIndex = sectorFrequency.OrderBy(kvp => kvp.Value).First().Key;
            var candidateSector = sectors[leastFrequentSectorIndex];
            
            // Standart çift/tek, yüksek/düşük analizini de yapalım
            // Geçmiş verilerden daha doğru eğilimleri tespit etmek için birkaç zaman penceresi kullanalım
            var windows = new[] { 30, 60, 100, 150 };
            var distributionTrends = new Dictionary<string, double>();
            
            foreach (var window in windows)
            {
                if (numbers.Count < window) continue;
                
                var slice = numbers.Take(window).ToList();
                double weight = 1.0 + (1.0 / window * 100); // Küçük pencerelere daha yüksek ağırlık ver
                
                // Çift/tek istatistikleri
                var oddCount = slice.Count(n => n % 2 == 1 && n > 0);
                var evenCount = slice.Count(n => n % 2 == 0 && n > 0);
                var zeroCount = slice.Count(n => n == 0);
                
                // Yüksek/düşük istatistikleri
                var lowCount = slice.Count(n => n > 0 && n <= 18);
                var highCount = slice.Count(n => n > 18);
                
                // Kırmızı/siyah istatistikleri
                var redNumberList = new List<int> { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
                var redCountWindow = slice.Count(n => redNumberList.Contains(n));
                var blackCountWindow = slice.Count(n => n > 0 && !redNumberList.Contains(n));
                
                // Beklenen oranlara göre dengesizlikleri hesapla
                distributionTrends["odd"] = distributionTrends.GetValueOrDefault("odd") + 
                                           ((oddCount / (double)(window - zeroCount) < 0.5) ? weight : -weight);
                                           
                distributionTrends["even"] = distributionTrends.GetValueOrDefault("even") + 
                                            ((evenCount / (double)(window - zeroCount) < 0.5) ? weight : -weight);
                                            
                distributionTrends["low"] = distributionTrends.GetValueOrDefault("low") + 
                                           ((lowCount / (double)(window - zeroCount) < 0.5) ? weight : -weight);
                                           
                distributionTrends["high"] = distributionTrends.GetValueOrDefault("high") + 
                                            ((highCount / (double)(window - zeroCount) < 0.5) ? weight : -weight);
                                            
                distributionTrends["red"] = distributionTrends.GetValueOrDefault("red") + 
                                           ((redCountWindow / (double)(window - zeroCount) < 0.5) ? weight : -weight);
                                           
                distributionTrends["black"] = distributionTrends.GetValueOrDefault("black") + 
                                              ((blackCountWindow / (double)(window - zeroCount) < 0.5) ? weight : -weight);
                                              
                distributionTrends["zero"] = distributionTrends.GetValueOrDefault("zero") + 
                                            ((zeroCount / (double)window < 0.027) ? weight : -weight);
            }

            // Kırmızı sayılar: 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36
            var redNumbers = new List<int> { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
            var redCount = recentNumbers.Count(n => redNumbers.Contains(n));
            var blackCount = recentNumbers.Count(n => n > 0 && !redNumbers.Contains(n));

            // 5. Sayı dizilerini analiz et (ardışık sayılar, belirli aralıklar)
            var sequences = AnalyzeSequences(numbers);

            // 6. Sayıların tekrarlanma aralıklarını analiz et
            var recurrenceIntervals = AnalyzeRecurrenceIntervals(numbers);

            // 5. GELİŞTİRME: Eksik sayı analizi
            // Son 100 çekiliste hiç çıkmayan veya çok az çıkan sayıları bul
            var last100 = numbers.Take(Math.Min(100, numbers.Count)).ToList();
            var missingNumbersScore = new Dictionary<int, double>();
            
            for (int num = 0; num <= 36; num++)
            {
                int count = last100.Count(n => n == num);
                int expectedCount = num == 0 ? 3 : 3; // 0 ve diğer sayıların beklenen frekansı
                
                if (count < expectedCount)
                {
                    // Eksik sayılara daha yüksek skor ver
                    missingNumbersScore[num] = 1.0 + (expectedCount - count) / (double)expectedCount;
                }
            }
            
            // 6. GELİŞTİRME: Komşu sayı analizi
            // Belirli sayıların yanında hangi sayıların çıktığını analiz et
            // Rulet çarkındaki fiziksel komşulukları dikkate al
            var wheelNumbers = new List<int> { 0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10, 5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26 };
            var numberPositions = new Dictionary<int, int>();
            
            for (int i = 0; i < wheelNumbers.Count; i++)
            {
                numberPositions[wheelNumbers[i]] = i;
            }
            
            var neighborTransitions = new Dictionary<int, Dictionary<int, int>>();
            foreach (var num in wheelNumbers)
            {
                neighborTransitions[num] = new Dictionary<int, int>();
            }
            
            // Sayılar listesindeki komşu geçişlerini hesapla
            for (int i = 0; i < numbers.Count - 1; i++)
            {
                int currentNum = numbers[i];
                int nextNum = numbers[i + 1];
                
                if (!neighborTransitions[currentNum].ContainsKey(nextNum))
                {
                    neighborTransitions[currentNum][nextNum] = 0;
                }
                
                neighborTransitions[currentNum][nextNum]++;
            }
            
            // Tüm stratejileri bir araya getirerek ağırlıklı bir tahmin yap
            // Standart tahmin metodu ile başlangıç tahmini al
            var prediction = GeneratePrediction(hotNumbers, coldNumbers, lastNumbers, 
                1, 1, 1, 1, 1, redCount, blackCount, // Basit dağılım değerleri
                sequences, recurrenceIntervals);
                
            // Çoklu Zaman Ölçekli Analiz kullanalım
            prediction = MultiTimeScaleAnalysis(
                numbers, 
                hotNumbers, 
                coldNumbers, 
                markovCandidates,
                distributionTrends);

            // TEST istatistiklerini toplama ve analiz etme kodu buraya eklenebilir
            // Hangi stratejinin daha isabetli olduğunu anlamak için

            return prediction;
        }

        private List<List<int>> AnalyzeSequences(List<int> numbers)
        {
            var result = new List<List<int>>();
            if (numbers == null || numbers.Count < 3)
            {
                return result;
            }

            // Sayılar listenin başında olduğu için ilk 25 sayıyı al (daha kapsamlı analiz için)
            var lastNumbers = numbers.Take(Math.Min(25, numbers.Count)).ToList();
            
            // 3'lü dizileri bul
            for (int i = 0; i < lastNumbers.Count - 2; i++)
            {
                var seq = new List<int> { lastNumbers[i], lastNumbers[i + 1], lastNumbers[i + 2] };
                
                // Bu dizi daha önce var mı kontrol et
                if (!result.Any(s => s.SequenceEqual(seq)))
                {
                    // Bu dizi başka yerde tekrar ediyor mu?
                    for (int j = i + 3; j < lastNumbers.Count - 2; j++)
                    {
                        if (lastNumbers[j] == seq[0] && 
                            j + 1 < lastNumbers.Count && lastNumbers[j + 1] == seq[1] &&
                            j + 2 < lastNumbers.Count && lastNumbers[j + 2] == seq[2])
                        {
                            result.Add(seq);
                            break;
                        }
                    }
                }
            }
            
            // 2'li dizileri de analiz et (daha kısa örüntüler için)
            for (int i = 0; i < lastNumbers.Count - 1; i++)
            {
                var seq = new List<int> { lastNumbers[i], lastNumbers[i + 1], -1 }; // -1 placeholder for prediction
                
                // Bu dizi başka yerde tekrar ediyor mu?
                for (int j = i + 2; j < lastNumbers.Count - 1; j++)
                {
                    if (lastNumbers[j] == seq[0] && j + 1 < lastNumbers.Count && lastNumbers[j + 1] == seq[1])
                    {
                        // Eğer bu ikili dizi sonrasında bir sayı varsa, onu tahmin olarak ekle
                        if (j + 2 < lastNumbers.Count)
                        {
                            seq[2] = lastNumbers[j + 2];
                            if (!result.Any(s => s[0] == seq[0] && s[1] == seq[1] && s[2] == seq[2]))
                            {
                                result.Add(new List<int>(seq));
                            }
                        }
                        break;
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Sayıların tekrarlanma aralıklarını analiz eder
        /// </summary>
        /// <param name="numbers">Analiz edilecek sayı listesi</param>
        /// <returns>Sayıların tekrarlanma aralıklarını içeren sözlük</returns>
        private Dictionary<int, int> AnalyzeRecurrenceIntervals(List<int> numbers)
        {
            var result = new Dictionary<int, int>();
            
            if (numbers == null || numbers.Count < 10)
            {
                return result;
            }
            
            // Her sayının tekrar etme aralığını hesapla
            var lastOccurrenceIndex = new Dictionary<int, int>();
            
            for (int i = 0; i < numbers.Count; i++)
            {
                int num = numbers[i];
                
                if (lastOccurrenceIndex.ContainsKey(num))
                {
                    int interval = i - lastOccurrenceIndex[num];
                    
                    if (result.ContainsKey(num))
                    {
                        // Mevcut aralık ile ortalama al
                        result[num] = (result[num] + interval) / 2;
                    }
                    else
                    {
                        result[num] = interval;
                    }
                }
                
                // Son görülme indeksini güncelle
                lastOccurrenceIndex[num] = i;
            }
            
            return result;
        }

        private int GeneratePrediction(
            List<int> hotNumbers, 
            List<int> coldNumbers, 
            List<int> lastNumbers,
            int oddCount,
            int evenCount,
            int zeroCount,
            int lowCount,
            int highCount,
            int redCount,
            int blackCount,
            List<List<int>> sequences,
            Dictionary<int, int> recurrenceIntervals)
        {
            var random = new Random(DateTime.Now.Millisecond); // Daha iyi rastgelelik için seed değerini değiştir
            var candidates = new List<int>();
            var weightedCandidates = new Dictionary<int, int>(); // Sayı ve ağırlık çifti
            
            // Tüm olası sayıları başlangıçta düşük ağırlıkla ekle (çeşitlilik için)
            for (int i = 0; i <= 36; i++)
            {
                weightedCandidates[i] = 1;
            }
            
            // Rastgele bir faktör belirle (her çağrıda farklı stratejilere ağırlık vermek için)
            double randomFactor = random.NextDouble();
            
            // Sıcak sayıları değerlendir (rastgele faktöre göre ağırlık ver)
            if (hotNumbers.Any())
            {
                int hotWeight = randomFactor < 0.5 ? 2 : 4; // Bazen daha fazla, bazen daha az ağırlık ver
                foreach (var num in hotNumbers)
                {
                    AddOrUpdateCandidate(weightedCandidates, num, hotWeight);
                }
            }
            
            // Dizi analizini değerlendir - sayılar listenin başında olduğu için ilk sayıları kontrol et
            if (sequences.Any() && lastNumbers.Count >= 2)
            {
                int seqMatchCount = 0;
                foreach (var seq in sequences)
                {
                    // Tam eşleşme kontrolü - son iki sayı bir dizinin başlangıcı mı?
                    if (seq.Count >= 3 && 
                        lastNumbers.Count >= 2 &&
                        lastNumbers[0] == seq[1] && 
                        lastNumbers[1] == seq[0])
                    {
                        // Dizi eşleşmesi bulundu, bu durumda seq[2] tahmin edilir
                        // Daha yüksek ağırlık ver çünkü bu güçlü bir örüntü
                        AddOrUpdateCandidate(weightedCandidates, seq[2], 6 + random.Next(1, 4)); // Rastgele ek ağırlık
                        seqMatchCount++;
                    }
                    
                    // Kısmi eşleşme kontrolü - son sayı bir dizinin parçası mı?
                    if (seq.Count >= 3 && lastNumbers.Count >= 1)
                    {
                        for (int i = 0; i < seq.Count - 1; i++)
                        {
                            if (lastNumbers[0] == seq[i])
                            {
                                // Kısmi eşleşme bulundu, bu durumda seq[i+1] tahmin edilir
                                AddOrUpdateCandidate(weightedCandidates, seq[i+1], 3 + random.Next(0, 3)); // Rastgele ek ağırlık
                                seqMatchCount++;
                                break;
                            }
                        }
                    }
                    
                    // Çok fazla dizi eşleşmesi varsa, bazılarını rastgele seç (çeşitlilik için)
                    if (seqMatchCount > 3 && random.NextDouble() > 0.7)
                    {
                        break;
                    }
                }
            }
            
            // Çift/tek dağılımını değerlendir
            double oddRatio = (double)oddCount / (oddCount + evenCount + zeroCount);
            double evenRatio = (double)evenCount / (oddCount + evenCount + zeroCount);
            double zeroRatio = (double)zeroCount / (oddCount + evenCount + zeroCount);
            
            // Beklenen oranlar: tek ~0.486, çift ~0.486, sıfır ~0.027
            // Rastgele bir strateji seç
            double strategyRandom = random.NextDouble();
            
            if (strategyRandom < 0.33) // Tek/çift stratejisi
            {
                if (oddRatio < 0.45)
                {
                    // Tek sayılar beklenen orandan daha az çıkmış
                    foreach (var num in Enumerable.Range(1, 36).Where(n => n % 2 == 1))
                    {
                        AddOrUpdateCandidate(weightedCandidates, num, 1 + random.Next(0, 2));
                    }
                }
                else if (evenRatio < 0.45)
                {
                    // Çift sayılar beklenen orandan daha az çıkmış
                    foreach (var num in Enumerable.Range(1, 36).Where(n => n % 2 == 0))
                    {
                        AddOrUpdateCandidate(weightedCandidates, num, 1 + random.Next(0, 2));
                    }
                }
            }
            else if (strategyRandom < 0.66) // Yüksek/düşük stratejisi
            {
                double lowRatio = (double)lowCount / (lowCount + highCount);
                if (lowRatio < 0.45)
                {
                    // Düşük sayılar az çıkmış
                    foreach (var num in Enumerable.Range(1, 18))
                    {
                        AddOrUpdateCandidate(weightedCandidates, num, 1 + random.Next(0, 2));
                    }
                }
                else
                {
                    // Yüksek sayılar az çıkmış
                    foreach (var num in Enumerable.Range(19, 18))
                    {
                        AddOrUpdateCandidate(weightedCandidates, num, 1 + random.Next(0, 2));
                    }
                }
            }
            else // Kırmızı/siyah stratejisi
            {
                // Kırmızı sayılar: 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36
                var redNumbers = new List<int> { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
                double redRatio = (double)redCount / (redCount + blackCount);
                
                if (redRatio < 0.45)
                {
                    // Kırmızı sayılar az çıkmış
                    foreach (var num in redNumbers)
                    {
                        AddOrUpdateCandidate(weightedCandidates, num, 1 + random.Next(0, 2));
                    }
                }
                else
                {
                    // Siyah sayılar az çıkmış
                    foreach (var num in Enumerable.Range(1, 36).Where(n => !redNumbers.Contains(n)))
                    {
                        AddOrUpdateCandidate(weightedCandidates, num, 1 + random.Next(0, 2));
                    }
                }
            }
            
            // 0 için özel durum
            if (zeroRatio < 0.02 && random.NextDouble() < 0.4)
            {
                AddOrUpdateCandidate(weightedCandidates, 0, 2 + random.Next(0, 3));
            }
            
            // Soğuk sayıları değerlendir (rastgele faktöre göre)
            if (coldNumbers.Any() && random.NextDouble() < 0.4)
            {
                // Rastgele birkaç soğuk sayı seç (hepsini değil)
                var selectedCold = coldNumbers.OrderBy(x => random.Next()).Take(Math.Min(5, coldNumbers.Count)).ToList();
                foreach (var num in selectedCold)
                {
                    AddOrUpdateCandidate(weightedCandidates, num, 1 + random.Next(1, 3));
                }
            }
            
            // Son 5 sayıyı kontrol et - bunların tekrar gelme olasılığı düşük
            if (lastNumbers.Count >= 5)
            {
                for (int i = 0; i < 5 && i < lastNumbers.Count; i++)
                {
                    if (weightedCandidates.ContainsKey(lastNumbers[i]))
                    {
                        // Son sayıların ağırlığını azalt, en son çıkan sayının tekrar gelme olasılığı en düşük
                        // Ancak bazen son sayıların tekrarlanma olasılığı da vardır
                        if (random.NextDouble() > 0.2) // %80 ihtimalle ceza uygula
                        {
                            int penalty = i == 0 ? 3 : (i <= 2 ? 2 : 1);
                            weightedCandidates[lastNumbers[i]] = Math.Max(1, weightedCandidates[lastNumbers[i]] - penalty);
                        }
                        else if (i > 2) // %20 ihtimalle ve son çıkanlardan değilse, tekrar gelebilir
                        {
                            weightedCandidates[lastNumbers[i]] += random.Next(1, 3);
                        }
                    }
                }
            }
            
            // Trend analizi: Son sayılarda artan veya azalan bir trend var mı?
            if (lastNumbers.Count >= 5 && random.NextDouble() < 0.6) // %60 ihtimalle trend analizi yap
            {
                bool increasingTrend = true;
                bool decreasingTrend = true;
                
                for (int i = 0; i < 4; i++)
                {
                    if (lastNumbers[i] <= lastNumbers[i + 1])
                        decreasingTrend = false;
                    if (lastNumbers[i] >= lastNumbers[i + 1])
                        increasingTrend = false;
                }
                
                if (increasingTrend && lastNumbers[0] < 30)
                {
                    // Artan trend varsa, daha büyük bir sayı tahmin et
                    // Ama rastgele bir aralık seç
                    int start = lastNumbers[0] + 1;
                    int range = Math.Min(6, 36 - start);
                    int selectedCount = Math.Min(range, 2 + random.Next(1, 4)); // 2-5 arası sayı seç
                    
                    var selectedNumbers = Enumerable.Range(start, range)
                        .OrderBy(x => random.Next())
                        .Take(selectedCount);
                        
                    foreach (var num in selectedNumbers)
                    {
                        AddOrUpdateCandidate(weightedCandidates, num, 2 + random.Next(1, 3));
                    }
                }
                else if (decreasingTrend && lastNumbers[0] > 6)
                {
                    // Azalan trend varsa, daha küçük bir sayı tahmin et
                    // Ama rastgele bir aralık seç
                    int end = lastNumbers[0] - 1;
                    int start = Math.Max(0, end - 6);
                    int range = end - start + 1;
                    int selectedCount = Math.Min(range, 2 + random.Next(1, 4)); // 2-5 arası sayı seç
                    
                    var selectedNumbers = Enumerable.Range(start, range)
                        .OrderBy(x => random.Next())
                        .Take(selectedCount);
                        
                    foreach (var num in selectedNumbers)
                    {
                        AddOrUpdateCandidate(weightedCandidates, num, 2 + random.Next(1, 3));
                    }
                }
            }
            
            // Tekrarlanma aralıkları stratejisi - sayıların gelme sıklığını değerlendir
            if (recurrenceIntervals.Any() && lastNumbers.Count > 0)
            {
                // Tekrarlanma aralığı bilinen ve yakın zamanda gelme potansiyeli olan sayıları değerlendir
                foreach (var kvp in recurrenceIntervals.OrderBy(k => k.Value)) // Aralığı en küçük olana öncelik ver
                {
                    int number = kvp.Key;
                    int interval = kvp.Value;
                    
                    // Son çıkan sayının ne kadar önce çıktığını bul
                    int lastOccurrence = -1;
                    for (int i = 0; i < lastNumbers.Count; i++)
                    {
                        if (lastNumbers[i] == number)
                        {
                            lastOccurrence = i;
                            break;
                        }
                    }
                    
                    // Eğer sayı son çıkan sayılardan biriyse
                    if (lastOccurrence != -1)
                    {
                        // Tekrarlanma aralığına bakarak ağırlık hesapla
                        // Örneğin sayı ortalama her 5 turda bir çıkıyorsa ve 4 tur önce çıktıysa, gelme ihtimali yüksek
                        int cycleProgress = lastOccurrence + 1; // Sayının son çıkışından bu yana geçen tur sayısı
                        double completionRatio = (double)cycleProgress / interval;
                        
                        // Tamamlanma oranına göre ağırlık ver
                        // 0.8-1.2 arasındaki tamamlanma oranları en yüksek ağırlığı alır
                        int weight = 0;
                        
                        if (completionRatio >= 0.8 && completionRatio <= 1.2)
                        {
                            // Sayı yakında çıkabilir, yüksek ağırlık ver
                            weight = 6 + random.Next(0, 3);
                        }
                        else if (completionRatio > 0.5 && completionRatio < 1.5)
                        {
                            // Orta ihtimal
                            weight = 3 + random.Next(0, 3);
                        }
                        else
                        {
                            // Düşük ihtimal
                            weight = 1 + random.Next(0, 2);
                        }
                        
                        // Düşük aralıklı sayılara bonus ver (sık tekrarlanan sayılar)
                        if (interval <= 5)
                        {
                            weight += 2;
                        }
                        else if (interval <= 10)
                        {
                            weight += 1;
                        }
                        
                        AddOrUpdateCandidate(weightedCandidates, number, weight);
                    }
                }
                
                // Standart komşu sayılar stratejisi (1 sağ, 1 sol) - bunu koruyalım istatistiksel çeşitlilik için
                if (random.NextDouble() < 0.3) // %30 ihtimalle uygula
                {
                    int lastNum = lastNumbers[0];
                    int neighbor1 = (lastNum + 1) % 37;
                    int neighbor2 = (lastNum + 36) % 37; // -1 mod 37
                    
                    AddOrUpdateCandidate(weightedCandidates, neighbor1, 1 + random.Next(0, 2));
                    AddOrUpdateCandidate(weightedCandidates, neighbor2, 1 + random.Next(0, 2));
                }
            }
            
            // Tamamen rastgele sayılar ekle (çeşitlilik için)
            for (int i = 0; i < 3; i++)
            {
                int randomNum = random.Next(0, 37);
                AddOrUpdateCandidate(weightedCandidates, randomNum, 1 + random.Next(0, 2));
            }
            
            // Ağırlıklı seçim yap
            if (weightedCandidates.Any())
            {
                // Rastgele seçim stratejisi belirle
                double selectionStrategy = random.NextDouble();
                
                if (selectionStrategy < 0.6) // %60 ihtimalle en yüksek ağırlıklı adaylardan seç
                {
                    // En yüksek ağırlıklı adayları bul
                    int maxWeight = weightedCandidates.Values.Max();
                    double threshold = 0.5 + (random.NextDouble() * 0.3); // 0.5-0.8 arası eşik değeri
                    var topCandidates = weightedCandidates.Where(kvp => kvp.Value >= maxWeight * threshold)
                                                          .Select(kvp => kvp.Key)
                                                          .ToList();
                    
                    if (topCandidates.Count > 0)
                    {
                        return topCandidates[random.Next(topCandidates.Count)];
                    }
                }
                
                // Ağırlıklı seçim için tüm adayları listeye ekle
                foreach (var kvp in weightedCandidates)
                {
                    for (int i = 0; i < kvp.Value; i++)
                    {
                        candidates.Add(kvp.Key);
                    }
                }
                
                if (candidates.Count > 0)
                {
                    return candidates[random.Next(candidates.Count)];
                }
            }
            
            // Hiçbir strateji uygulanamadıysa, rastgele bir sayı döndür
            return random.Next(37); // 0-36 arası
        }
        
        private void AddOrUpdateCandidate(Dictionary<int, int> candidates, int number, int weight)
        {
            if (candidates.ContainsKey(number))
            {
                candidates[number] += weight;
            }
            else
            {
                candidates[number] = weight;
            }
        }

        #endregion

        #region Tahmin Doğruluk Takibi ve Strateji Analizi
        
        /// <summary>
        /// Gerçek çıkan sayıyı sisteme bildirir ve tahmin doğruluğunu günceller
        /// </summary>
        /// <param name="actualNumber">Gerçekte çıkan sayı</param>
        /// <returns>Doğruluk güncelleme sonucu</returns>
        public async Task<PredictionAccuracyResponse> RecordActualNumberAsync(int actualNumber)
        {
            try
            {
                if (_lastPredictedNumber == -1)
                {
                    return new PredictionAccuracyResponse 
                    { 
                        Success = false, 
                        ErrorMessage = "Henüz bir tahmin yapılmamış. Önce AddNumberAndPredict metodunu çağırın." 
                    };
                }
                
                // Rulet verilerini al
                var rouletteData = await GetRouletteDataAsync();
                if (rouletteData == null || rouletteData.Numbers == null || rouletteData.Numbers.Count == 0)
                {
                    return new PredictionAccuracyResponse 
                    { 
                        Success = false, 
                        ErrorMessage = "Rulet verileri bulunamadı veya boş." 
                    };
                }
                
                // Tahmin sonucunu kaydet
                var predictionResult = new PredictionResult
                {
                    PredictedNumber = _lastPredictedNumber,
                    ActualNumber = actualNumber,
                    PredictionTime = DateTime.Now,
                    NumbersUsed = new List<int>(rouletteData.Numbers),
                    StrategyContributions = new Dictionary<string, int>()
                };
                
                // Her strateji için katkıyı ekle - gerçek strateji katkıları için GeneratePrediction'ı güncellememiz gerekecek
                // Şimdilik varsayılan değerler ekliyoruz
                foreach (var strategyName in _strategyNames)
                {
                    predictionResult.StrategyContributions[strategyName] = 5; // Varsayılan orta ağırlık
                }
                
                // Sonucu MongoDB'ye kaydet
                await _predictionResultsCollection.InsertOneAsync(predictionResult);
                
                // Strateji performanslarını güncelle
                foreach (var strategyName in _strategyNames)
                {
                    var strategy = await _strategyPerformanceCollection.Find(s => s.StrategyName == strategyName).FirstOrDefaultAsync();
                    
                    if (strategy != null)
                    {
                        // Kullanım sayısını arttır ve doğruysa doğru tahmin sayısını da arttır
                        strategy.UsageCount++;
                        
                        // Tahmin edilen sayının 9 sağ ve 9 sol komşuları da doğru kabul edilecek
                        int rightNeighbor = (_lastPredictedNumber + 9) % 37; // 9 sağ komşu
                        int leftNeighbor = (_lastPredictedNumber + 28) % 37; // (currentNumber - 9 + 37) % 37 ile aynı, 9 sol komşu
                        
                        // Tam eşleşme, sağ komşu eşleşmesi veya sol komşu eşleşmesi olursa doğru say
                        bool isCorrect = _lastPredictedNumber == actualNumber || rightNeighbor == actualNumber || leftNeighbor == actualNumber;
                        
                        if (isCorrect)
                        {
                            strategy.CorrectPredictionCount++;
                        }
                        
                        // Son sonuçları güncelle
                        if (strategy.RecentResults.Count >= 100)
                        {
                            strategy.RecentResults.RemoveAt(0); // En eski sonucu çıkar
                        }
                        strategy.RecentResults.Add(isCorrect);
                        
                        // Dinamik ağırlığı son performansa göre güncelle
                        // Basit bir dinamik ayarlama: son 10 sonucun doğruluk oranına göre ağırlık ayarla
                        if (strategy.RecentResults.Count >= 10)
                        {
                            int recentCorrect = strategy.RecentResults.TakeLast(10).Count(r => r);
                            double recentAccuracy = (double)recentCorrect / 10;
                            
                            // Ağırlığı güncelle (20-80 aralığında)
                            strategy.DynamicWeight = Math.Max(20, Math.Min(80, (int)(recentAccuracy * 100)));
                        }
                        
                        strategy.LastUpdated = DateTime.Now;
                        
                        // MongoDB'yi güncelle
                        await _strategyPerformanceCollection.ReplaceOneAsync(s => s.Id == strategy.Id, strategy);
                    }
                }
                
                // Sonuçları döndür
                return await GetPredictionAccuracyAsync();
            }
            catch (Exception ex)
            {
                return new PredictionAccuracyResponse
                {
                    Success = false,
                    ErrorMessage = $"Tahmin sonucu kaydedilirken hata oluştu: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Tüm tahmin stratejilerinin performansını getirir
        /// </summary>
        /// <returns>Strateji performans sonuçları</returns>
        public async Task<List<StrategyPerformance>> GetStrategyPerformancesAsync()
        {
            try
            {
                return await _strategyPerformanceCollection.Find(_ => true).ToListAsync();
            }
            catch (Exception)
            {
                return new List<StrategyPerformance>();
            }
        }

        /// <summary>
        /// Genel tahmin doğruluk oranlarını getirir
        /// </summary>
        /// <returns>Genel doğruluk analizi</returns>
        public async Task<PredictionAccuracyResponse> GetPredictionAccuracyAsync()
        {
            try
            {
                // Tüm tahmin sonuçlarını getir
                var allPredictions = await _predictionResultsCollection.Find(_ => true).ToListAsync();
                var totalPredictions = allPredictions.Count;
                
                if (totalPredictions == 0)
                {
                    return new PredictionAccuracyResponse
                    {
                        Success = true,
                        TotalPredictions = 0,
                        CorrectPredictions = 0,
                        Last10Accuracy = 0,
                        Last50Accuracy = 0,
                        Last100Accuracy = 0,
                        MostSuccessfulStrategy = "Henüz veri yok",
                        MostSuccessfulStrategyAccuracy = 0,
                        StrategyPerformances = new List<StrategyPerformanceSummary>(),
                        Last10Results = new List<int>()
                    };
                }
                
                // Doğru tahminlerin sayısını hesapla
                var correctPredictions = allPredictions.Count(p => p.IsCorrect);
                
                // Son 10, 50 ve 100 tahminin doğruluk oranlarını hesapla
                var last10 = allPredictions.OrderByDescending(p => p.PredictionTime).Take(10).ToList();
                var last50 = allPredictions.OrderByDescending(p => p.PredictionTime).Take(50).ToList();
                var last100 = allPredictions.OrderByDescending(p => p.PredictionTime).Take(100).ToList();
                
                var last10Accuracy = last10.Count > 0 ? (double)last10.Count(p => p.IsCorrect) / last10.Count : 0;
                var last50Accuracy = last50.Count > 0 ? (double)last50.Count(p => p.IsCorrect) / last50.Count : 0;
                var last100Accuracy = last100.Count > 0 ? (double)last100.Count(p => p.IsCorrect) / last100.Count : 0;
                
                // Strateji performanslarını getir
                var strategies = await GetStrategyPerformancesAsync();
                
                // En başarılı stratejiyi bul
                var mostSuccessful = strategies.OrderByDescending(s => s.RecentAccuracyRate).FirstOrDefault();
                
                // Son 10 sonucu listele (1: doğru, 0: yanlış)
                var last10Results = last10.Select(p => p.IsCorrect ? 1 : 0).ToList();
                
                // Strateji performans özetlerini oluştur
                var strategyPerformances = strategies.Select(s => new StrategyPerformanceSummary
                {
                    StrategyName = s.StrategyName,
                    AccuracyRate = s.AccuracyRate,
                    CurrentWeight = s.DynamicWeight
                }).ToList();
                
                return new PredictionAccuracyResponse
                {
                    Success = true,
                    TotalPredictions = totalPredictions,
                    CorrectPredictions = correctPredictions,
                    Last10Accuracy = last10Accuracy,
                    Last50Accuracy = last50Accuracy,
                    Last100Accuracy = last100Accuracy,
                    MostSuccessfulStrategy = mostSuccessful?.StrategyName ?? "Veri yetersiz",
                    MostSuccessfulStrategyAccuracy = mostSuccessful?.AccuracyRate ?? 0,
                    StrategyPerformances = strategyPerformances,
                    Last10Results = last10Results
                };
            }
            catch (Exception ex)
            {
                return new PredictionAccuracyResponse
                {
                    Success = false,
                    ErrorMessage = $"Tahmin doğruluk analizi yapılırken hata oluştu: {ex.Message}"
                };
            }
        }
        
        /// <summary>
        /// Çoklu Zaman Ölçekli Analiz metodu ile tahmin yapar
        /// </summary>
        /// <param name="numbers">Tüm sayılar listesi</param>
        /// <param name="hotNumbers">Sıcak sayılar listesi</param>
        /// <param name="coldNumbers">Soğuk sayılar listesi</param>
        /// <param name="markovCandidates">Markov adayları</param>
        /// <param name="distributionTrends">Dağılım trendleri</param>
        /// <returns>Tahmini sayı</returns>
        private int MultiTimeScaleAnalysis(
            List<int> numbers,
            List<int> hotNumbers,
            List<int> coldNumbers,
            Dictionary<int, double> markovCandidates,
            Dictionary<string, double> distributionTrends)
        {
            // Farklı zaman ölçekleri (pencere boyutları)
            var timeScales = new[] { 5, 10, 20, 50, 100 };
            var scaleWeights = new Dictionary<int, double> { { 5, 1.0 }, { 10, 0.9 }, { 20, 0.8 }, { 50, 0.7 }, { 100, 0.6 } };
            
            // Sonuçları toplamak için sözlük
            var numbersScore = new Dictionary<int, double>();
            
            // Her olasi sayı için başlangıç skoru belirle
            for (int num = 0; num <= 36; num++)
            {
                numbersScore[num] = 1.0; // Başlangıç değeri
            }
            
            // Her zaman ölçeği için işlem yap
            foreach (var scale in timeScales)
            {
                // Eğer yeterli veri yoksa, bu ölçeği atla
                if (numbers.Count < scale) continue;
                
                // Bu zaman ölçeği için sayıları al
                var scaleNumbers = numbers.Take(scale).ToList();
                
                // Bu ölçek için frekans analizi
                var frequencyAnalysis = AnalyzeFrequencyInTimeScale(scaleNumbers);
                
                // Bu ölçek için dizi analizi
                var patternAnalysis = AnalyzePatternsInTimeScale(scaleNumbers);
                
                // Bu ölçek için dağılım analizi
                var distributionAnalysis = AnalyzeDistributionInTimeScale(scaleNumbers);
                
                // Skorları birleştir
                foreach (var kvp in frequencyAnalysis)
                {
                    numbersScore[kvp.Key] += kvp.Value * scaleWeights[scale];
                }
                
                foreach (var kvp in patternAnalysis)
                {
                    if (numbersScore.ContainsKey(kvp.Key))
                    {
                        numbersScore[kvp.Key] += kvp.Value * scaleWeights[scale] * 1.5; // Desenlere daha yüksek ağırlık ver
                    }
                }
                
                foreach (var kvp in distributionAnalysis)
                {
                    if (numbersScore.ContainsKey(kvp.Key))
                    {
                        numbersScore[kvp.Key] += kvp.Value * scaleWeights[scale] * 1.2;
                    }
                }
            }
            
            // Sıcak ve soğuk sayılarla entegrasyon
            // Sıcak sayıların skorunu artır
            foreach (var num in hotNumbers)
            {
                numbersScore[num] *= 1.3;
            }
            
            // Soğuk sayıların skorunu azalt
            foreach (var num in coldNumbers)
            {
                numbersScore[num] *= 0.8;
            }
            
            // Markov tahminleriyle entegrasyon
            foreach (var kvp in markovCandidates)
            {
                numbersScore[kvp.Key] += kvp.Value * 2.0; // Markov adaylarına yüksek ağırlık ver
            }
            
            // Dağılım trendleriyle entegrasyon
            if (distributionTrends.ContainsKey("odd") && distributionTrends["odd"] > 0)
            {
                for (int i = 1; i <= 35; i += 2)
                {
                    numbersScore[i] += distributionTrends["odd"] * 0.5;
                }
            }
            
            if (distributionTrends.ContainsKey("even") && distributionTrends["even"] > 0)
            {
                for (int i = 2; i <= 36; i += 2)
                {
                    numbersScore[i] += distributionTrends["even"] * 0.5;
                }
            }
            
            if (distributionTrends.ContainsKey("low") && distributionTrends["low"] > 0)
            {
                for (int i = 1; i <= 18; i++)
                {
                    numbersScore[i] += distributionTrends["low"] * 0.5;
                }
            }
            
            if (distributionTrends.ContainsKey("high") && distributionTrends["high"] > 0)
            {
                for (int i = 19; i <= 36; i++)
                {
                    numbersScore[i] += distributionTrends["high"] * 0.5;
                }
            }
            
            // En yüksek skora sahip sayıyı seç
            return numbersScore.OrderByDescending(kvp => kvp.Value).First().Key;
        }
        
        /// <summary>
        /// Belirli bir zaman ölçeğinde sayıların frekansını analiz eder
        /// </summary>
        private Dictionary<int, double> AnalyzeFrequencyInTimeScale(List<int> numbers)
        {
            var result = new Dictionary<int, double>();
            var frequency = new Dictionary<int, int>();
            
            // Frekansları hesapla
            foreach (var num in numbers)
            {
                if (frequency.ContainsKey(num))
                {
                    frequency[num]++;
                }
                else
                {
                    frequency[num] = 1;
                }
            }
            
            // Frekansları skorlara dönüştür
            double maxFreq = frequency.Any() ? frequency.Values.Max() : 1;
            
            foreach (var kvp in frequency)
            {
                // Sıklığa göre ağırlıklandır
                result[kvp.Key] = kvp.Value / maxFreq;
            }
            
            // Tüm olası sayılar için bir temel değer ata (0-36)
            for (int i = 0; i <= 36; i++)
            {
                if (!result.ContainsKey(i))
                {
                    result[i] = 0.1; // Hiç görülmemiş sayılar için düşük bir başlangıç değeri
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Belirli bir zaman ölçeğinde desenleri analiz eder
        /// </summary>
        private Dictionary<int, double> AnalyzePatternsInTimeScale(List<int> numbers)
        {
            var result = new Dictionary<int, double>();
            
            // Tüm olası sayılar için başlangıç değeri ata
            for (int i = 0; i <= 36; i++)
            {
                result[i] = 0.1;
            }
            
            if (numbers.Count < 3) return result; // En az 3 sayı gerekli
            
            // Son 2 sayıyı al
            var last2 = numbers.Take(2).ToList();
            
            // 2'li desenleri ara
            for (int i = 2; i < numbers.Count; i++)
            {
                // Son 2 sayı ile eşleşen bir desen var mı?
                if (numbers[i-2] == last2[0] && numbers[i-1] == last2[1])
                {
                    // Desen bulundu, bir sonraki sayının skorunu artır
                    int nextNum = numbers[i];
                    result[nextNum] += 1.0;
                    
                    // Komşu sayıların skorunu da artır (9 sağ/9 sol komsu kuralı)
                    for (int neighbor = 1; neighbor <= 9; neighbor++)
                    {
                        int rightNeighbor = (nextNum + neighbor) % 37; // 37'ye göre mod alarak 0-36 aralığında tut
                        int leftNeighbor = (nextNum - neighbor + 37) % 37; // Negatif değerleri önlemek için 37 ekle
                        
                        result[rightNeighbor] += 0.3 * (1.0 - (neighbor / 10.0)); // Uzaklık arttıkça etki azalsın
                        result[leftNeighbor] += 0.3 * (1.0 - (neighbor / 10.0));
                    }
                }
            }
            
            // Tek sayı desenleri ara
            for (int i = 1; i < numbers.Count; i++)
            {
                // Son sayı ile eşleşen sayılar var mı?
                if (numbers[i-1] == last2[0])
                {
                    // Eşleşme bulundu, bir sonraki sayının skorunu artır
                    int nextNum = numbers[i];
                    result[nextNum] += 0.5;
                    
                    // Komşu sayıların skorunu da artır (daha az etki)
                    for (int neighbor = 1; neighbor <= 5; neighbor++) // Tek desenlerde daha az komşu sayı
                    {
                        int rightNeighbor = (nextNum + neighbor) % 37;
                        int leftNeighbor = (nextNum - neighbor + 37) % 37;
                        
                        result[rightNeighbor] += 0.1 * (1.0 - (neighbor / 6.0));
                        result[leftNeighbor] += 0.1 * (1.0 - (neighbor / 6.0));
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Belirli bir zaman ölçeğinde sayı dağılımını analiz eder
        /// </summary>
        private Dictionary<int, double> AnalyzeDistributionInTimeScale(List<int> numbers)
        {
            var result = new Dictionary<int, double>();
            
            // Tüm olası sayılar için başlangıç değeri ata
            for (int i = 0; i <= 36; i++)
            {
                result[i] = 0.1;
            }
            
            if (numbers.Count < 5) return result; // Yeterli veri yok
            
            // Tek/Çift dağılımı
            int oddCount = numbers.Count(n => n % 2 == 1 && n > 0);
            int evenCount = numbers.Count(n => n % 2 == 0 && n > 0);
            
            // Düşük/Yüksek dağılımı
            int lowCount = numbers.Count(n => n >= 1 && n <= 18);
            int highCount = numbers.Count(n => n >= 19 && n <= 36);
            
            // Kırmızı/Siyah dağılımı
            var redNumbers = new List<int> { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
            int redCount = numbers.Count(n => redNumbers.Contains(n));
            int blackCount = numbers.Count(n => n > 0 && !redNumbers.Contains(n));
            
            // Sıfır sayısı
            int zeroCount = numbers.Count(n => n == 0);
            
            // Dağılımlara göre skorları güncelle
            
            // Eğer tek sayılar daha azsa, tek sayıların skorunu artır
            if (oddCount < evenCount * 0.8)
            {
                for (int i = 1; i <= 35; i += 2)
                {
                    result[i] += 0.5;
                }
            }
            // Eğer çift sayılar daha azsa, çift sayıların skorunu artır
            else if (evenCount < oddCount * 0.8)
            {
                for (int i = 2; i <= 36; i += 2)
                {
                    result[i] += 0.5;
                }
            }
            
            // Eğer düşük sayılar daha azsa, düşük sayıların skorunu artır
            if (lowCount < highCount * 0.8)
            {
                for (int i = 1; i <= 18; i++)
                {
                    result[i] += 0.5;
                }
            }
            // Eğer yüksek sayılar daha azsa, yüksek sayıların skorunu artır
            else if (highCount < lowCount * 0.8)
            {
                for (int i = 19; i <= 36; i++)
                {
                    result[i] += 0.5;
                }
            }
            
            // Eğer kırmızı sayılar daha azsa, kırmızı sayıların skorunu artır
            if (redCount < blackCount * 0.8)
            {
                foreach (var num in redNumbers)
                {
                    result[num] += 0.5;
                }
            }
            // Eğer siyah sayılar daha azsa, siyah sayıların skorunu artır
            else if (blackCount < redCount * 0.8)
            {
                for (int i = 1; i <= 36; i++)
                {
                    if (!redNumbers.Contains(i))
                    {
                        result[i] += 0.5;
                    }
                }
            }
            
            // Eğer sıfır görülmemişse ve yeterli veri varsa, sıfırın skorunu artır
            if (zeroCount == 0 && numbers.Count >= 20)
            {
                result[0] += 1.0;
            }
            
            return result;
        }
        
        #endregion
        
        /// <summary>
        /// Tahminin doğruluğunu kontrol eder (9-sağ/9-sol komşu kuralı)
        /// </summary>
        /// <param name="prediction">Tahmin edilen sayı</param>
        /// <param name="actual">Gerçek çıkan sayı</param>
        /// <returns>Tahmin doğru ise true, yanlış ise false</returns>
        private bool IsCorrectPrediction(int prediction, int actual)
        {
            // Tahmin doğrudan doğru mu?
            if (prediction == actual) return true;
            
            // Tahmin edilen sayının komşularını hesapla
            var neighbors = CalculateNeighbors(prediction);
            
            // Gerçek sayı komşular içinde mi?
            return neighbors.Contains(actual);
        }
        
        /// <summary>
        /// Bir sayının 9-sağ/9-sol komşularını hesaplar
        /// </summary>
        /// <param name="number">Komşuları hesaplanacak sayı</param>
        /// <returns>Komşu sayılar dizisi</returns>
        private int[] CalculateNeighbors(int number)
        {
            var neighbors = new List<int>();
            
            // Önce sayının çark üzerindeki pozisyonunu bul
            int position = Array.IndexOf(_wheelSequence, number);
            
            if (position == -1)
            {
                // Eğer sayı çark üzerinde bulunamazsa boş dizi dön
                return Array.Empty<int>();
            }
            
            // Kendisi
            neighbors.Add(number);
            
            // 9-sağ komşu (saat yönünde)
            for (int i = 1; i <= 9; i++)
            {
                int rightPosition = (position + i) % _wheelSequence.Length;
                neighbors.Add(_wheelSequence[rightPosition]);
            }
            
            // 9-sol komşu (saat yönünün tersinde)
            for (int i = 1; i <= 9; i++)
            {
                int leftPosition = (position - i);
                if (leftPosition < 0) leftPosition += _wheelSequence.Length;
                neighbors.Add(_wheelSequence[leftPosition]);
            }
            
            return neighbors.ToArray();
        }
    }
}
