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
                _strategyPerformanceCollection = database.GetCollection<StrategyPerformance>("strategy_performance");
                
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
                    new RecentNumbersPenaltyStrategy(),
                    new MonteCarloSimulationStrategy(),
                    new MachineLearningStrategy(),
                    new SectorBasedAnalysisStrategy(),
                    new MarkovChainStrategy(),
                    new TrigramAnalysisStrategy(),
                    new BayesianAnalysisStrategy(),
                    new TemporalAnalysisStrategy(),
                    new HybridPredictionStrategy(),
                    new MotionVectorStrategy(),
                    new GoldenRatioStrategy(),
                    new IntuitiveAnalysisStrategy()
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
        /// <returns>Tahmin edilen sayı, strateji adı ve en iyi 3 strateji</returns>
        public async Task<(int prediction, string strategyName, List<Models.Roulette.TopStrategyPrediction> topStrategies)> PredictNextNumberAsync(List<int> numbers)
        {
            if (numbers == null || numbers.Count == 0)
            {
                return (-1, "Veri Yok", new List<Models.Roulette.TopStrategyPrediction>());
            }
            
            try
            {
                // Tüm stratejilere tahmin yaptır ve veritabanına kaydet
                var allPredictions = await PredictWithAllStrategiesInternalAsync(numbers);
                
                // En başarılı 3 stratejiyi bul
                var topStrategies = await FindTopPerformingStrategiesAsync(3);
                var topStrategyPredictions = new List<Models.Roulette.TopStrategyPrediction>();
                
                // Top strategy predictions oluştur
                foreach (var (strategyName, successRate) in topStrategies)
                {
                    if (allPredictions.ContainsKey(strategyName))
                    {
                        topStrategyPredictions.Add(new Models.Roulette.TopStrategyPrediction
                        {
                            StrategyName = strategyName,
                            PredictedNumber = allPredictions[strategyName],
                            SuccessRate = Math.Round(successRate * 100, 2) // Yüzde olarak başarı oranı
                        });
                    }
                }
                
                // En başarılı stratejiyi bul
                var bestStrategy = await FindBestPerformingStrategyAsync();
                
                if (bestStrategy != null)
                {
                    // En başarılı stratejinin ismini ve başarı oranını kaydet (debug için)
                    Console.WriteLine($"En başarılı strateji: {bestStrategy.StrategyName} - Başarı oranı: {(double)bestStrategy.CorrectPredictionCount / bestStrategy.UsageCount:P2}");
                    
                    // Tüm tahminler arasından en başarılı stratejinin tahmini döndür
                    if (allPredictions.ContainsKey(bestStrategy.StrategyName))
                    {
                        return (allPredictions[bestStrategy.StrategyName], bestStrategy.StrategyName, topStrategyPredictions);
                    }
                }
                else 
                {
                    Console.WriteLine("En başarılı strateji bulunamadı!");
                }
                
                // En başarılı strateji bulunamadığında ağırlıklı tahmin kullan
                int weightedPrediction = await GetWeightedPredictionAsync(allPredictions);
                return (weightedPrediction, "Ağırlıklı Tahmin", topStrategyPredictions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tahmin yaparken hata: {ex.Message}");
                return (-1, "Hata", new List<Models.Roulette.TopStrategyPrediction>());
            }  
        }
        
        /// <summary>
        /// Tüm stratejileri kullanarak tahmin yapar ve tüm tahminleri kaydeder
        /// </summary>
        /// <param name="numbers">Rulet sayıları</param>
        /// <returns>Ağırlıklı tahmin sonuçları</returns>
        public async Task<int> PredictWithAllStrategiesAsync(List<int> numbers)
        {
            try 
            {
                // Tüm stratejileri tahmin ettir ve kaydet
                var predictionResults = await PredictWithAllStrategiesInternalAsync(numbers);
                
                // Ağırlıklı bir sonuç döndür
                return await GetWeightedPredictionAsync(predictionResults);
            }
            catch (Exception)
            {
                // Herhangi bir hata durumunda, 0-36 arası rastgele bir sayı döndür
                return new Random().Next(0, 37);
            }
        }
        
        /// <summary>
        /// Tüm stratejileri kullanarak tahmin yapar ve tüm tahminleri kaydeder fakat sonuç döndürmez (sadece arka planda çalışır)
        /// </summary>
        /// <param name="numbers">Rulet sayıları</param>
        public async Task PredictWithAllStrategiesWithoutResultAsync(List<int> numbers)
        {
            try
            {
                // Tüm stratejileri tahmin ettir ve kaydet
                await PredictWithAllStrategiesInternalAsync(numbers);
            }
            catch (Exception)
            {
                // Hata durumunda sessizce devam et
            }
        }
        
        /// <summary>
        /// Tüm stratejileri kullanarak tahmin yapar (iç metot, dışa kapalı)
        /// </summary>
        /// <param name="numbers">Rulet sayıları</param>
        /// <returns>Dictionary<string, int> tahmin sonuçları, Strateji adı -> tahmin edilen sayı</returns>
        private async Task<Dictionary<string, int>> PredictWithAllStrategiesInternalAsync(List<int> numbers)
        {
            // Tüm stratejiler için tahminleri topla
            var predictionTasks = new List<Task<int>>();
            var predictionResults = new Dictionary<string, int>(); // Strateji adı -> tahmin edilen sayı
            
            foreach (var strategy in _strategies)
            {
                predictionTasks.Add(Task.Run(() =>
                {
                    var prediction = strategy.PredictNextNumber(numbers);
                    return prediction;
                }));
            }
            
            // Tüm tahminleri bekle
            await Task.WhenAll(predictionTasks);
            
            // Tahmin sonuçlarını topla
            for (int i = 0; i < _strategies.Count; i++)
            {
                predictionResults[_strategies[i].Name] = predictionTasks[i].Result;
            }
            
            // Her strateji için bir tahmin kaydı oluştur ve MongoDB'ye kaydet
            var saveTasks = new List<Task>();
            foreach (var strategy in _strategies)
            {
                int predictedNumber = predictionResults[strategy.Name];
                var neighbors = CalculateNeighbors(predictedNumber);
                
                // Oluşturulan tahmini veritabanına kaydet
                var record = new PredictionRecord
                {
                    PredictionDate = DateTime.Now,
                    Strategy = strategy.Name,
                    PredictedNumber = predictedNumber,
                    ActualNumber = null, // henüz bilinmiyor
                    IsCorrect = null, // henüz bilinmiyor
                    Neighbors = neighbors
                };
                
                saveTasks.Add(_predictionRecordsCollection.InsertOneAsync(record));
            }
            
            // Tüm kayıtları paralel olarak kaydet
            await Task.WhenAll(saveTasks);
            
            return predictionResults;
        }
        
        /// <summary>
        /// [DEPRECATED] En başarılı strateji ile yeni tahmin yapar - Artık bu metot kullanılmıyor, yerine PredictNextNumberAsync kullan
        /// </summary>
        /// <param name="numbers">Rulet sayıları</param>
        /// <returns>En başarılı stratejinin tahmini</returns>
        public async Task<int> PredictWithBestStrategyAsync(List<int> numbers)
        {
            // Bu metot artık doğrudan PredictNextNumberAsync'ye yönleniyor
            var result = await PredictNextNumberAsync(numbers);
            return result.prediction;
        }
        
        /// <summary>
        /// Mevcut verilere göre en başarılı stratejiyi bulur - artık kullanım sayısı sınırı yok
        /// </summary>
        /// <returns>En başarılı strateji performansı</returns>
        private async Task<StrategyPerformance> FindBestPerformingStrategyAsync()
        {
            try
            {
                // Tüm stratejilerin performans kayıtlarını getir
                var performances = await _strategyPerformanceCollection
                    .Find(_ => true)
                    .ToListAsync();
                
                if (performances == null || performances.Count == 0)
                    return null;
                
                // En yüksek başarı oranına sahip stratejiyi bul - kullanım sayısı sınırı kaldırıldı
                var bestPerformers = performances
                    .Where(p => p.UsageCount > 0) // sadece en az 1 kez kullanılmış olmalı (sıfıra bölünme hatası önlemek için)
                    .OrderByDescending(p => (double)p.CorrectPredictionCount / p.UsageCount); // başarı oranına göre sırala
                
                return bestPerformers.Any() ? bestPerformers.First() : null;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Mevcut verilere göre en başarılı N stratejiyi bulur
        /// </summary>
        /// <param name="count">Kaç strateji döndürüleceği</param>
        /// <returns>En başarılı stratejilerin listesi</returns>
        private async Task<List<(string strategyName, double successRate)>> FindTopPerformingStrategiesAsync(int count)
        {
            try
            {
                // Tüm stratejilerin performans kayıtlarını getir
                var performances = await _strategyPerformanceCollection
                    .Find(_ => true)
                    .ToListAsync();
                
                if (performances == null || performances.Count == 0)
                    return new List<(string, double)>();
                
                // En yüksek başarı oranına sahip stratejileri bul
                var topPerformers = performances
                    .Where(p => p.UsageCount > 0) // sadece en az 1 kez kullanılmış olmalı (sıfıra bölünme hatası önlemek için)
                    .Select(p => (p.StrategyName, SuccessRate: (double)p.CorrectPredictionCount / p.UsageCount))
                    .OrderByDescending(p => p.SuccessRate) // başarı oranına göre sırala
                    .Take(count)
                    .ToList();
                
                return topPerformers;
            }
            catch
            {
                return new List<(string, double)>();
            }
        }

        /// <summary>
        /// Ağırlıklı bir tahmin sonucu hesaplar
        /// </summary>
        /// <param name="predictions">Tahminler</param>
        /// <returns>Ağırlıklı tahmin</returns>
        private async Task<int> GetWeightedPredictionAsync(Dictionary<string, int> predictions)
        {
            try
            {
                // Tüm strateji performanslarını getir
                var performances = await _strategyPerformanceCollection
                    .Find(_ => true)
                    .ToListAsync();
                
                // Her tahmin için ağırlıklı oy hesapla
                var weightedVotes = new Dictionary<int, double>();
                
                foreach (var prediction in predictions)
                {
                    string strategyName = prediction.Key;
                    int predictedNumber = prediction.Value;
                    
                    // Stratejinin ağırlığını bul
                    var performance = performances.FirstOrDefault(p => p.StrategyName == strategyName);
                    double weight = performance?.DynamicWeight ?? 50; // Eğer yoksa varsayılan ağırlık
                    
                    // Bu sayıya oy ekle
                    if (!weightedVotes.ContainsKey(predictedNumber))
                    {
                        weightedVotes[predictedNumber] = 0;
                    }
                    
                    weightedVotes[predictedNumber] += weight;
                }
                
                // En yüksek ağırlıklı oyu bul
                if (weightedVotes.Count > 0)
                {
                    return weightedVotes.OrderByDescending(wv => wv.Value).First().Key;
                }
                
                // Eğer hiç oy yoksa, en sık tahmin edilen sayıyı döndür
                return GetMostPredictedNumber(predictions.Values.ToList());
            }
            catch
            {
                // Hata durumunda, tahminlerden rastgele birini seç
                if (predictions.Count > 0)
                {
                    return predictions.Values.ElementAt(new Random().Next(predictions.Count));
                }
                
                return new Random().Next(0, 37);
            }
        }
        
        /// <summary>
        /// En çok tahmin edilen sayıyı bulur
        /// </summary>
        /// <param name="predictions">Tahminler</param>
        /// <returns>En çok tahmin edilen sayı</returns>
        private int GetMostPredictedNumber(List<int> predictions)
        {
            if (predictions == null || predictions.Count == 0)
            {
                return new Random().Next(0, 37); // Tahmin yoksa rastgele döndür
            }
            
            // Frekansı hesapla
            var frequency = new Dictionary<int, int>();
            
            foreach (var number in predictions)
            {
                if (!frequency.ContainsKey(number))
                {
                    frequency[number] = 0;
                }
                
                frequency[number]++;
            }
            
            // En çok tekrar eden sayıyı bul
            return frequency.Any() ? frequency.OrderByDescending(f => f.Value).First().Key : new Random().Next(0, 37);
        }

        /// <summary>
        /// Bir önceki tahminin doğruluğunu kontrol eder ve tüm ilgili stratejilerin kayıtlarını günceller
        /// </summary>
        /// <param name="actualNumber">Gerçek sayı</param>
        public async Task UpdatePredictionAccuracyAsync(int actualNumber)
        {
            try
            {
                // Son tahmin sonuçlarını (henüz gerçekleşmemiş) getir
                var predictionRecords = await _predictionRecordsCollection
                    .Find(r => r.ActualNumber == null) // henüz gerçekleşmemiş tahminler
                    .ToListAsync();
                
                if (predictionRecords == null || predictionRecords.Count == 0)
                {
                    return;
                }
                
                // Her strateji için tahmin doğruluğunu kontrol et
                foreach (var record in predictionRecords)
                {
                    // Bu stratejiyi bul
                    IPredictionStrategy strategy = _strategies.FirstOrDefault(s => s.Name == record.Strategy);
                    
                    if (strategy == null)
                    {
                        continue;
                    }
                    
                    // Tahminin komşuları
                    int[] neighbors = record.Neighbors;
                    
                    // MERKEZİ DOĞRULUK KONTROLÜ - tüm stratejiler için aynı mantık
                    // Gerçek sayı komşular içinde mi? (komşular tahmin edilen sayının kendisini de içerir)
                    bool isCorrect = neighbors != null && neighbors.Contains(actualNumber);
                    
                    // İşlemleri atomik olarak yapmak için doğrudan kayıtları güncelle
                    record.ActualNumber = actualNumber;
                    record.IsCorrect = isCorrect; // Merkezi doğruluk kontrolü - strateji mantığından bağımsız
                    
                    // Kaydı güncelle
                    await _predictionRecordsCollection.ReplaceOneAsync(
                        r => r.Id == record.Id,
                        record
                    );
                    
                    // Strateji performansını güncelle
                    await UpdateStrategyPerformanceAsync(record.Strategy, record.IsCorrect ?? false);
                }
            }
            catch
            {
                // Hata durumunda sessizce devam et
            }
        }
        
        /// <summary>
        /// Strateji performans kayıtlarını günceller
        /// </summary>
        /// <param name="strategyName">Strateji adı</param>
        /// <param name="isCorrect">Doğru tahmin mi?</param>
        private async Task UpdateStrategyPerformanceAsync(string strategyName, bool isCorrect)
        {
            try
            {
                // Strateji performans kaydını bul
                var performance = await _strategyPerformanceCollection
                    .Find(p => p.StrategyName == strategyName)
                    .FirstOrDefaultAsync();
                
                if (performance == null)
                {
                    // Performans kaydı yoksa yeni oluştur
                    performance = new StrategyPerformance
                    {
                        StrategyName = strategyName,
                        UsageCount = 0,
                        CorrectPredictionCount = 0,
                        DynamicWeight = 50, // Başlangıçta eşit ağırlık
                        LastUpdated = DateTime.Now,
                        RecentResults = new List<bool>()
                    };
                }
                
                // Performans kayıtlarını güncelle
                performance.UsageCount++;
                
                if (isCorrect)
                {
                    performance.CorrectPredictionCount++;
                }
                
                // Son 10 sonucu sakla
                if (performance.RecentResults == null)
                {
                    performance.RecentResults = new List<bool>();
                }
                
                performance.RecentResults.Insert(0, isCorrect);
                
                // Sadece son 10 sonucu tut
                if (performance.RecentResults.Count > 10)
                {
                    performance.RecentResults = performance.RecentResults.Take(10).ToList();
                }
                
                // Yeni ağırlık hesapla
                if (performance.UsageCount > 0)
                {
                    double successRate = (double)performance.CorrectPredictionCount / performance.UsageCount;
                    
                    // Son 10 sonucun başarı oranı (daha yeni sonuçlar daha önemli)
                    double recentSuccessRate = performance.RecentResults.Count > 0
                        ? performance.RecentResults.Count(r => r) / (double)performance.RecentResults.Count
                        : 0;
                        
                    // Ağırlık hesapla: genel başarı %30, son başarı %70 etkili
                    performance.DynamicWeight = (int)((successRate * 100 * 0.3) + (recentSuccessRate * 100 * 0.7));
                    performance.LastUpdated = DateTime.Now;
                    
                    // Null kontrolü ekle
                    performance.RecentResults ??= new List<bool>();
                    
                    // Veritabanını güncelle
                    await _strategyPerformanceCollection.ReplaceOneAsync(
                        p => p.StrategyName == strategyName,
                        performance
                    );
                }
            }
            catch
            {
                // Hata durumunda sessizce devam et
            }
        }
        
        /// <summary>
        /// Bir sayının 9-sağ/9-sol komşularını ve kendisini hesaplar
        /// </summary>
        /// <param name="number">Sayı</param>
        /// <returns>Sayının kendisi ve komşu sayılar (toplamda 19 sayı)</returns>
        private int[] CalculateNeighbors(int number)
        {
            int[] neighbors = new int[19]; // 9 sağ + 9 sol + kendisi = 19 sayı
            
            // Çarkta sayının pozisyonunu bul
            int position = Array.IndexOf(_wheelSequence, number);
            
            // Eğer sayı çarkta bulunamazsa, boş dizi döndür
            if (position == -1)
            {
                return new int[0];
            }
            
            // Sayının kendisini ortaya ekle (9. indeks)
            neighbors[9] = number;
            
            // 9 sol komşuyu hesapla
            for (int i = 1; i <= 9; i++)
            {
                int leftPosition = (position - i + _wheelSequence.Length) % _wheelSequence.Length;
                neighbors[9 - i] = _wheelSequence[leftPosition];
            }
            
            // 9 sağ komşuyu hesapla
            for (int i = 1; i <= 9; i++)
            {
                int rightPosition = (position + i) % _wheelSequence.Length;
                neighbors[9 + i] = _wheelSequence[rightPosition];
            }
            
            return neighbors;
        }
        
        /// <summary>
        /// Bir sayının belirli sayıda komşusunu ve kendisini alır
        /// </summary>
        /// <param name="number">Merkez sayı</param>
        /// <param name="neighborCount">Tek yönde komşu sayısı</param>
        /// <returns>Sayının kendisi ve komşu sayılar (toplamda 2*neighborCount+1 sayı)</returns>
        private int[] GetNeighbors(int number, int neighborCount)
        {
            int[] neighbors = new int[neighborCount * 2 + 1]; // sol + sağ komşular + kendisi
            
            // Çarkta sayının pozisyonunu bul
            int position = Array.IndexOf(_wheelSequence, number);
            
            // Eğer sayı çarkta bulunamazsa, boş dizi döndür
            if (position == -1)
            {
                return new int[0];
            }
            
            // Sayının kendisini ortaya ekle
            neighbors[neighborCount] = number;
            
            // Sol komşuları hesapla
            for (int i = 1; i <= neighborCount; i++)
            {
                int leftPosition = (position - i + _wheelSequence.Length) % _wheelSequence.Length;
                neighbors[neighborCount - i] = _wheelSequence[leftPosition];
            }
            
            // Sağ komşuları hesapla
            for (int i = 1; i <= neighborCount; i++)
            {
                int rightPosition = (position + i) % _wheelSequence.Length;
                neighbors[neighborCount + i] = _wheelSequence[rightPosition];
            }
            
            return neighbors;
        }
    }
}
