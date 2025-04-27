using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using CaseStudy.Application.Models.Roulette;
using Microsoft.Extensions.Options;

namespace CaseStudy.Application.Strategies
{
    /// <summary>
    /// Tüm tahmin stratejilerini yöneten ve tahminleri kaydeden yönetici sınıf
    /// </summary>
    public class StrategyManager
    {
        private readonly List<IPredictionStrategy> _strategies;
        private readonly IMongoCollection<PredictionRecord> _predictionRecordsCollection;
        private readonly IMongoCollection<StrategyPerformance> _strategyPerformanceCollection;
        
        // Rulet çarkındaki sayıların fiziksel dizilimi (saat yönünde)
        private readonly int[] _wheelSequence = new int[] {
            0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10, 5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
        };

        /// <summary>
        /// StrategyManager constructor
        /// </summary>
        /// <param name="mongoSettings">MongoDB ayarları</param>
        public StrategyManager(IOptions<MongoDBSettings> mongoSettings)
        {
            try
            {
                var client = new MongoClient(mongoSettings.Value.ConnectionString);
                var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
                _predictionRecordsCollection = database.GetCollection<PredictionRecord>(mongoSettings.Value.PredictionRecordsCollectionName);
                _strategyPerformanceCollection = database.GetCollection<StrategyPerformance>(mongoSettings.Value.StrategyPerformanceCollectionName);
                
                // Tüm tahmin stratejilerini ekle
                _strategies = new List<IPredictionStrategy>
                {
                    new HotNumbersStrategy(),
                    new ColdNumbersStrategy(),
                    new OddEvenDistributionStrategy(),
                    new HighLowDistributionStrategy(),
                    new RedBlackDistributionStrategy(),
                    new SequenceAnalysisStrategy(),
                    new RecurrenceIntervalsStrategy(),
                    new RecentNumbersPenaltyStrategy()
                };
                
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
                foreach (var strategy in _strategies)
                {
                    var strategyRecord = await _strategyPerformanceCollection.Find(s => s.StrategyName == strategy.Name).FirstOrDefaultAsync();
                    
                    if (strategyRecord == null)
                    {
                        // Yeni strateji kaydı oluştur
                        await _strategyPerformanceCollection.InsertOneAsync(new StrategyPerformance
                        {
                            StrategyName = strategy.Name,
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

        /// <summary>
        /// Tüm stratejileri kullanarak bir sonraki sayıyı tahmin eder ve veritabanına kaydeder
        /// </summary>
        /// <param name="numbers">Rulet sayıları</param>
        /// <returns>Tahmin edilen sayı</returns>
        public async Task<int> PredictNextNumberAsync(List<int> numbers)
        {
            if (numbers == null || numbers.Count == 0)
            {
                return -1;
            }
            
            // Tüm stratejilerin ağırlıklarını al
            var strategyWeights = await GetStrategyWeightsAsync();
            
            // Her stratejinin tahminini al ve kaydet
            var predictions = new Dictionary<string, int>();
            var allPredictedNumbers = new List<int>();
            
            foreach (var strategy in _strategies)
            {
                try
                {
                    // Strateji ile tahmin yap
                    int predictedNumber = strategy.PredictNextNumber(numbers);
                    predictions[strategy.Name] = predictedNumber;
                    allPredictedNumbers.Add(predictedNumber);
                    
                    // Tahminin komşularını hesapla
                    var neighbors = CalculateNeighbors(predictedNumber);
                    
                    // Tahmin kaydını oluştur ve veritabanına kaydet
                    var predictionRecord = new PredictionRecord
                    {
                        PredictionDate = DateTime.UtcNow,
                        PredictedNumber = predictedNumber,
                        ActualNumber = null, // Henüz bilinmiyor, bir sonraki tahmin isteğinde güncellenecek
                        IsCorrect = null, // Henüz bilinmiyor
                        Context = numbers.Take(5).ToArray(), // Son 5 sayı (bağlam)
                        Strategy = strategy.Name,
                        Neighbors = neighbors
                    };
                    
                    await _predictionRecordsCollection.InsertOneAsync(predictionRecord);
                }
                catch (Exception)
                {
                    // Strateji hata verirse, o stratejiyi atla
                    continue;
                }
            }
            
            // Ağırlıklı karar verme - En çok tahmin edilen sayıyı veya en yüksek ağırlıklı stratejinin tahminini döndür
            int finalPrediction;
            
            // Stratejilerin ağırlıklarına göre karar ver
            if (strategyWeights.Count > 0)
            {
                // En yüksek performansa sahip stratejinin tahminini bul
                var bestStrategy = strategyWeights.OrderByDescending(kv => kv.Value).First().Key;
                if (predictions.ContainsKey(bestStrategy))
                {
                    finalPrediction = predictions[bestStrategy];
                }
                else
                {
                    // En sık tahmin edilen sayıyı bul
                    finalPrediction = GetMostPredictedNumber(allPredictedNumbers);
                }
            }
            else
            {
                // Ağırlık bilgisi yoksa, en sık tahmin edilen sayıyı bul
                finalPrediction = GetMostPredictedNumber(allPredictedNumbers);
            }
            
            return finalPrediction;
        }

        /// <summary>
        /// En çok tahmin edilen sayıyı bulur
        /// </summary>
        private int GetMostPredictedNumber(List<int> predictions)
        {
            if (predictions == null || predictions.Count == 0)
            {
                return new Random(DateTime.Now.Millisecond).Next(0, 37);
            }
            
            // En çok tekrarlanan sayıyı bul
            var mostFrequent = predictions
                .GroupBy(p => p)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => Guid.NewGuid()) // Eşitlik durumunda rastgele seç
                .Select(g => g.Key)
                .FirstOrDefault();
                
            return mostFrequent;
        }

        /// <summary>
        /// Strateji performanslarını alır ve dinamik ağırlıkları hesaplar
        /// </summary>
        private async Task<Dictionary<string, double>> GetStrategyWeightsAsync()
        {
            var result = new Dictionary<string, double>();
            
            try
            {
                var performances = await _strategyPerformanceCollection.Find(Builders<StrategyPerformance>.Filter.Empty)
                                                                       .ToListAsync();
                
                foreach (var perf in performances)
                {
                    double successRate = perf.UsageCount > 0 
                        ? (double)perf.CorrectPredictionCount / perf.UsageCount 
                        : 0.5; // Başlangıç oranı
                        
                    // Son 10 sonucun ağırlığını artır (daha güncel performans)
                    if (perf.RecentResults.Count > 0)
                    {
                        double recentSuccessRate = (double)perf.RecentResults.Count(r => r) / perf.RecentResults.Count;
                        // Son sonuçlar %60, genel performans %40 ağırlıklı
                        successRate = (successRate * 0.4) + (recentSuccessRate * 0.6);
                    }
                    
                    // Dinamik ağırlık, başarı oranına ve kısmen de önceki ağırlığa bağlı
                    double dynamicWeight = (successRate * 100 * 0.7) + (perf.DynamicWeight * 0.3);
                    result[perf.StrategyName] = dynamicWeight;
                }
            }
            catch (Exception)
            {
                // Hata durumunda tüm stratejilere eşit ağırlık ver
                foreach (var strategy in _strategies)
                {
                    result[strategy.Name] = 50.0;
                }
            }
            
            return result;
        }

        /// <summary>
        /// Bir önceki tahminin doğruluğunu kontrol eder ve tüm ilgili stratejilerin kayıtlarını günceller
        /// </summary>
        /// <param name="actualNumber">Gerçek sayı</param>
        public async Task UpdatePredictionAccuracyAsync(int actualNumber)
        {
            try
            {
                // En son iki tahmin kaydını bul
                var allRecords = await _predictionRecordsCollection.Find(Builders<PredictionRecord>.Filter.Empty)
                                                     .Sort(Builders<PredictionRecord>.Sort.Descending(pr => pr.PredictionDate))
                                                     .Limit(20) // Her strateji için son kaydı alabilmek için biraz fazla al
                                                     .ToListAsync();
                
                // Stratejilere göre gruplama yap ve her strateji için en son tahmini bul
                var lastPredictionsByStrategy = allRecords
                    .GroupBy(pr => pr.Strategy)
                    .ToDictionary(
                        g => g.Key, 
                        g => g.OrderByDescending(pr => pr.PredictionDate).First()
                    );
                
                // Her strateji için kontrol et ve güncelle
                foreach (var strategy in _strategies)
                {
                    if (lastPredictionsByStrategy.TryGetValue(strategy.Name, out var predictionRecord))
                    {
                        // Stratejinin doğruluk kontrolünü yap
                        bool isCorrect = strategy.CheckPredictionAccuracy(
                            predictionRecord.PredictedNumber, 
                            actualNumber,
                            predictionRecord.Neighbors
                        );
                        
                        // Prediksiyon kaydını güncelle
                        predictionRecord.ActualNumber = actualNumber;
                        predictionRecord.IsCorrect = isCorrect;
                        
                        await _predictionRecordsCollection.ReplaceOneAsync(
                            Builders<PredictionRecord>.Filter.Eq(pr => pr.Id, predictionRecord.Id),
                            predictionRecord);
                            
                        // Strateji performans kaydını güncelle
                        await UpdateStrategyPerformanceAsync(strategy.Name, isCorrect);
                    }
                }
            }
            catch (Exception)
            {
                // Hata durumunda işlemi sessizce geç
            }
        }

        /// <summary>
        /// Strateji performans kayıtlarını günceller
        /// </summary>
        private async Task UpdateStrategyPerformanceAsync(string strategyName, bool isCorrect)
        {
            try
            {
                var performance = await _strategyPerformanceCollection.Find(s => s.StrategyName == strategyName).FirstOrDefaultAsync();
                
                if (performance != null)
                {
                    // Kullanım sayısını artır
                    performance.UsageCount++;
                    
                    // Doğru tahmin sayısını güncelle
                    if (isCorrect)
                    {
                        performance.CorrectPredictionCount++;
                    }
                    
                    // Son sonuçları güncelle
                    if (performance.RecentResults == null)
                    {
                        performance.RecentResults = new List<bool>();
                    }
                    
                    performance.RecentResults.Add(isCorrect);
                    
                    // Sadece son 10 sonucu tut
                    if (performance.RecentResults.Count > 10)
                    {
                        performance.RecentResults = performance.RecentResults.Skip(performance.RecentResults.Count - 10).ToList();
                    }
                    
                    // Dinamik ağırlığı güncelle
                    double successRate = (double)performance.CorrectPredictionCount / performance.UsageCount;
                    double recentSuccessRate = performance.RecentResults.Count > 0
                        ? (double)performance.RecentResults.Count(r => r) / performance.RecentResults.Count
                        : 0;
                        
                    // Ağırlık hesapla: genel başarı %30, son başarı %70 etkili
                    performance.DynamicWeight = (int)((successRate * 100 * 0.3) + (recentSuccessRate * 100 * 0.7));
                    performance.LastUpdated = DateTime.Now;
                    
                    // Veritabanını güncelle
                    await _strategyPerformanceCollection.ReplaceOneAsync(
                        Builders<StrategyPerformance>.Filter.Eq(s => s.Id, performance.Id),
                        performance);
                }
            }
            catch (Exception)
            {
                // Hata durumunda işlemi sessizce geç
            }
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
