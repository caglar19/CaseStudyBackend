using System.Collections.Generic;
using System.Threading.Tasks;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Interfaces
{
    public interface IRouletteService
    {
        /// <summary>
        /// Rulet tahmin işlemini gerçekleştirir.
        /// İlk çağrıda initialNumbers ile başlatılır, sonraki çağrılarda newNumber ile tahmin yapar.
        /// </summary>
        /// <param name="initialNumbers">İlk yüklemede kullanılacak rulet sayıları (ilk kez gönderildiğinde)</param>
        /// <param name="newNumber">Yeni gelen rulet sayısı (sonraki isteklerde)</param>
        /// <returns>Tahmin sonucu</returns>
        Task<RoulettePredictionResponse> PredictRoulette(List<int>? initialNumbers, int? newNumber);
    }
}
