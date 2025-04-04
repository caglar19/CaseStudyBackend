using System.Collections.Generic;
using System.Threading.Tasks;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Interfaces
{
    public interface IRouletteService
    {
        /// <summary>
        /// İlk rulet sayılarını yükler
        /// </summary>
        /// <param name="initialNumbers">İlk yüklenecek rulet sayıları</param>
        /// <returns>Yükleme sonucu</returns>
        Task<RouletteInitializeResponse> InitializeWithNumbers(List<int> initialNumbers);
        
        /// <summary>
        /// Yeni rulet sayısı ekler ve bir sonraki sayıyı tahmin eder
        /// </summary>
        /// <param name="newNumber">Yeni gelen rulet sayısı</param>
        /// <returns>Tahmin sonucu</returns>
        Task<RoulettePredictionResponse> AddNumberAndPredict(int newNumber);
        
        /// <summary>
        /// HTML içeriğinden rulet sayılarını çıkarır
        /// </summary>
        /// <param name="htmlContent">Rulet sayılarını içeren HTML içeriği</param>
        /// <returns>Çıkarılan rulet sayıları</returns>
        Task<RouletteExtractNumbersResponse> ExtractNumbersFromHtml(string htmlContent);
    }
}
