using System.Collections.Generic;
using System.Threading.Tasks;
using CaseStudy.Application.Models.Roulette;

// Kullanılacak modelleri açıkça belirt
using RoulettePredictionResponseModel = CaseStudy.Application.Models.Roulette.RoulettePredictionResponse;
using RouletteInitializeResponseModel = CaseStudy.Application.Models.Roulette.RouletteInitializeResponse;
using RouletteExtractNumbersResponseModel = CaseStudy.Application.Models.Roulette.RouletteExtractNumbersResponse;

namespace CaseStudy.Application.Interfaces
{
    public interface IRouletteService
    {
        /// <summary>
        /// İlk rulet sayılarını yükler
        /// </summary>
        /// <param name="initialNumbers">İlk yüklenecek rulet sayıları</param>
        /// <returns>Yükleme sonucu</returns>
        Task<RoulettePredictionResponseModel> InitializeNumbersAsync(List<int> initialNumbers);
        
        /// <summary>
        /// Yeni rulet sayısı ekler ve bir sonraki sayıyı tahmin eder
        /// </summary>
        /// <param name="newNumber">Yeni gelen rulet sayısı</param>
        /// <returns>Tahmin sonucu</returns>
        Task<RoulettePredictionResponseModel> AddNumberAndPredict(int newNumber);
        
        /// <summary>
        /// HTML içeriğinden rulet sayılarını çıkarır
        /// </summary>
        /// <param name="htmlContent">Rulet sayılarını içeren HTML içeriği</param>
        /// <returns>Çıkarılan rulet sayıları</returns>
        Task<RouletteExtractNumbersResponseModel> ExtractNumbersFromHtml(string htmlContent);
        
        /// <summary>
        /// Gerçek çıkan sayıyı sisteme bildirir ve tahmin doğruluğunu günceller
        /// </summary>
        /// <param name="actualNumber">Gerçekte çıkan sayı</param>
        /// <returns>Doğruluk güncelleme sonucu</returns>
        Task<PredictionAccuracyResponse> RecordActualNumberAsync(int actualNumber);
        
        /// <summary>
        /// Tüm tahmin stratejilerinin performansını getirir
        /// </summary>
        /// <returns>Strateji performans sonuçları</returns>
        Task<List<StrategyPerformance>> GetStrategyPerformancesAsync();
        
        /// <summary>
        /// Genel tahmin doğruluk oranlarını getirir
        /// </summary>
        /// <returns>Genel doğruluk analizi</returns>
        Task<PredictionAccuracyResponse> GetPredictionAccuracyAsync();
    }
}
