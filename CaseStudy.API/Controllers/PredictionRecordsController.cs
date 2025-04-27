using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using CaseStudy.Application.Models.Roulette;
using Microsoft.AspNetCore.Authorization;

namespace CaseStudy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PredictionRecordsController : ControllerBase
    {
        private readonly IMongoCollection<PredictionRecord> _predictionRecordsCollection;
        
        public PredictionRecordsController(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _predictionRecordsCollection = database.GetCollection<PredictionRecord>(mongoSettings.Value.PredictionRecordsCollectionName);
        }
        
        /// <summary>
        /// Son tahmin kayıtlarını getirir
        /// </summary>
        /// <param name="limit">Döndürülecek kayıt sayısı</param>
        /// <returns>Tahmin kayıtları</returns>
        [HttpGet]
        public async Task<IActionResult> GetPredictionRecords([FromQuery] int limit = 100)
        {
            try
            {
                var records = await _predictionRecordsCollection
                    .Find(_ => true)
                    .SortByDescending(r => r.PredictionDate)
                    .Limit(limit)
                    .ToListAsync();
                
                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Tahmin kayıtları alınırken hata oluştu: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Tüm stratejilerin tahmin sonuçlarını detaylı olarak getirir
        /// </summary>
        /// <param name="limit">Her strateji için değerlendirilecek son kayıt sayısı</param>
        /// <returns>Her stratejinin ayrı ayrı tahmin sonuçları, 1=doğru, 0=yanlış</returns>
        [HttpGet("strategies/performance")]
        public async Task<IActionResult> GetStrategyPerformances([FromQuery] int limit = 500)
        {
            try
            {
                // Sonuçları belli olan tüm kayıtları getir
                var records = await _predictionRecordsCollection
                    .Find(r => r.ActualNumber != null)
                    .SortByDescending(r => r.PredictionDate)
                    .Limit(limit)
                    .ToListAsync();
                
                // Tüm stratejileri bul
                var strategies = records.Select(r => r.Strategy).Distinct().ToList();
                
                // Her strateji için ayrı ayrı tahmin sonuçlarını topla
                var strategyResults = new Dictionary<string, object>();
                
                foreach (var strategy in strategies)
                {
                    var strategyRecords = records.Where(r => r.Strategy == strategy)
                        .OrderByDescending(r => r.PredictionDate)
                        .Take(limit)
                        .ToList();
                    
                    if (strategyRecords.Count == 0)
                        continue;
                    
                    // İstatistiksel özet 
                    int exactMatches = strategyRecords.Count(r => r.ActualNumber == r.PredictedNumber);
                    int neighborMatches = strategyRecords.Count(r => r.IsCorrect == true && r.ActualNumber != r.PredictedNumber);
                    int totalMatches = exactMatches + neighborMatches;
                    int failedMatches = strategyRecords.Count - totalMatches;
                    
                    // Tüm tahmin sonuçlarını liste olarak getir (1=Doğru, 0=Yanlış)
                    var predictionResults = strategyRecords.Select(r => new {
                        Date = r.PredictionDate,
                        Predicted = r.PredictedNumber,
                        Actual = r.ActualNumber,
                        Result = r.IsCorrect == true ? 1 : 0, // True olanı 1, false olanı 0 olarak göster
                        ExactMatch = r.ActualNumber == r.PredictedNumber ? 1 : 0 // Tam isabeti de ayrıca göster
                    }).ToList();
                    
                    // Son 500 tahmin sonuçları dizisi (1 veya 0) - grafik çizmek için kolay format
                    var resultArray = strategyRecords
                        .Take(500)
                        .Select(r => r.IsCorrect == true ? 1 : 0)
                        .ToArray();
                    
                    // Her strateji için sonuçları birleştir
                    strategyResults[strategy] = new {
                        StrategyName = strategy,
                        TotalPredictions = strategyRecords.Count,
                        ExactMatches = exactMatches,
                        NeighborMatches = neighborMatches,
                        TotalSuccess = totalMatches,
                        FailedPredictions = failedMatches,
                        SuccessRate = strategyRecords.Count > 0 ? (double)totalMatches / strategyRecords.Count : 0,
                        ExactSuccessRate = strategyRecords.Count > 0 ? (double)exactMatches / strategyRecords.Count : 0,
                        LastUpdated = strategyRecords.FirstOrDefault()?.PredictionDate,
                        Results = predictionResults,  // Tüm tahmin sonuçları (detaylı)
                        ResultsArray = resultArray     // Basit sonuç dizisi (1 veya 0)
                    };
                }
                
                // Stratejilerin performans sıralamasını da ekle
                var rankedStrategies = strategyResults.Values
                    .Cast<dynamic>()
                    .OrderByDescending(s => s.SuccessRate)
                    .Select((s, index) => new { Rank = index + 1, StrategyName = s.StrategyName, SuccessRate = s.SuccessRate })
                    .ToList();
                
                var response = new {
                    StrategyResults = strategyResults,
                    Rankings = rankedStrategies,
                    TotalStrategies = strategies.Count,
                    GeneratedAt = DateTime.Now
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Strateji performansları hesaplanırken hata oluştu: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Belirtilen stratejiye ait tahmin kayıtlarını getirir
        /// </summary>
        /// <param name="strategyName">Strateji adı</param>
        /// <param name="limit">Döndürülecek kayıt sayısı</param>
        /// <returns>Strateji tahmin kayıtları</returns>
        [HttpGet("strategies/{strategyName}")]
        public async Task<IActionResult> GetStrategyRecords(string strategyName, [FromQuery] int limit = 100)
        {
            try
            {
                var records = await _predictionRecordsCollection
                    .Find(r => r.Strategy == strategyName)
                    .SortByDescending(r => r.PredictionDate)
                    .Limit(limit)
                    .ToListAsync();
                
                if (records.Count == 0)
                {
                    return NotFound($"\"{strategyName}\" stratejisine ait kayıt bulunamadı.");
                }
                
                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Strateji kayıtları alınırken hata oluştu: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Tüm strateji isimlerini listeler
        /// </summary>
        [HttpGet("strategies")]
        public async Task<IActionResult> GetStrategies()
        {
            try
            {
                // Tüm strateji isimlerini çek
                var strategies = await _predictionRecordsCollection
                    .Distinct(r => r.Strategy, _ => true)
                    .ToListAsync();
                
                // Stratejileri alfabetik sırala
                return Ok(strategies.OrderBy(s => s));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Stratejiler listelenirken hata oluştu: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Tüm stratejilerin ayrı ayrı başarı istatistiklerini getirir
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                // Tüm kayıtları getir
                var allRecords = await _predictionRecordsCollection
                    .Find(r => r.ActualNumber != null) // Sadece sonuçları belli olan tahminler
                    .ToListAsync();
                
                // Genel istatistikler
                var generalStats = new
                {
                    TotalPredictions = allRecords.Count,
                    ExactMatches = allRecords.Count(r => r.ActualNumber == r.PredictedNumber),
                    NeighborMatches = allRecords.Count(r => r.IsCorrect == true && r.ActualNumber != r.PredictedNumber),
                    FailedPredictions = allRecords.Count(r => r.IsCorrect == false)
                };
                
                // Tüm stratejileri bul
                var strategies = allRecords.Select(r => r.Strategy).Distinct().ToList();
                
                // Her strateji için istatistikler
                var strategyStatsList = new List<object>();
                
                foreach (var strategy in strategies)
                {
                    var strategyRecords = allRecords.Where(r => r.Strategy == strategy)
                        .OrderByDescending(r => r.PredictionDate)
                        .ToList();
                        
                    if (strategyRecords.Count == 0)
                        continue;
                    
                    // Tam doğru tahmin sayısı (direkt aynı sayı)
                    int exactMatches = strategyRecords.Count(r => r.ActualNumber == r.PredictedNumber);
                    
                    // Komşu sayılarla doğru tahmin (9-sağ/9-sol komşu)
                    int neighborMatches = strategyRecords.Count(r => r.IsCorrect == true && r.ActualNumber != r.PredictedNumber);
                    
                    // Başarısız tahminler
                    int failedMatches = strategyRecords.Count(r => r.IsCorrect == false);
                    
                    // Son 3 gün içindeki kayıtlar
                    var recentRecords = strategyRecords.Where(r => r.PredictionDate >= DateTime.Now.AddDays(-3)).ToList();
                    var recentExactMatches = recentRecords.Count(r => r.ActualNumber == r.PredictedNumber);
                    var recentNeighborMatches = recentRecords.Count(r => r.IsCorrect == true && r.ActualNumber != r.PredictedNumber);
                    var recentSuccessRate = recentRecords.Count > 0 
                        ? (double)(recentExactMatches + recentNeighborMatches) / recentRecords.Count 
                        : 0;
                        
                    // Tüm tahmin sonuçlarını 1/0 dizisi olarak hazırla - yan yana görünecek
                    var resultsArray = strategyRecords
                        .Select(r => r.IsCorrect == true ? 1 : 0)
                        .ToArray();
                    
                    var strategyStats = new
                    {
                        StrategyName = strategy,
                        TotalPredictions = strategyRecords.Count,
                        ExactMatches = exactMatches,
                        NeighborMatches = neighborMatches,
                        TotalSuccesses = exactMatches + neighborMatches,
                        FailedPredictions = failedMatches,
                        ExactSuccessRate = Math.Round((double)exactMatches / strategyRecords.Count, 2),
                        OverallSuccessRate = Math.Round((double)(exactMatches + neighborMatches) / strategyRecords.Count, 2),
                        RecentSuccessRate = Math.Round(recentSuccessRate, 2),
                        LastPrediction = strategyRecords.First().PredictionDate,
                        ResultsArray = resultsArray // Yan yana başarı sonuçları: 1=doğru 0=yanlış
                    };
                    
                    strategyStatsList.Add(strategyStats);
                }
                
                // Stratejileri başarı oranına göre sırala
                strategyStatsList = strategyStatsList.OrderByDescending(s => ((dynamic)s).OverallSuccessRate).ToList();
                
                var statistics = new
                {
                    GeneralStatistics = generalStats,
                    StrategyStatistics = strategyStatsList,
                    TotalStrategies = strategies.Count,
                    LastUpdated = DateTime.Now
                };
                
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"İstatistikler hesaplanırken hata oluştu: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Strateji performans istatistikleri
    /// </summary>
    public class StrategyPerformanceStats
    {
        /// <summary>
        /// Strateji adı
        /// </summary>
        public string StrategyName { get; set; }
        
        /// <summary>
        /// Toplam tahmin sayısı
        /// </summary>
        public int TotalPredictions { get; set; }
        
        /// <summary>
        /// Tam doğru tahmin sayısı (birebir aynı sayı)
        /// </summary>
        public int ExactMatches { get; set; }
        
        /// <summary>
        /// Komşu doğru tahmin sayısı (9-sağ/9-sol komşu)
        /// </summary>
        public int NeighborMatches { get; set; }
        
        /// <summary>
        /// Toplam başarılı tahmin sayısı (tam + komşu)
        /// </summary>
        public int TotalSuccess { get; set; }
        
        /// <summary>
        /// Tam doğru tahmin başarı oranı (0-1 arası)
        /// </summary>
        public double ExactSuccessRate { get; set; }
        
        /// <summary>
        /// Genel başarı oranı (tam + komşu, 0-1 arası)
        /// </summary>
        public double OverallSuccessRate { get; set; }
        
        /// <summary>
        /// Son güncelleme tarihi
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// Son tahmin sonuçları
        /// </summary>
        public object RecentResults { get; set; }
    }
}
