using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    /// <summary>
    /// Rulet tahmin stratejilerinin tanımlandığı arayüz
    /// </summary>
    public interface IPredictionStrategy
    {
        /// <summary>
        /// Stratejinin adı
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Bir sonraki sayıyı tahmin eder
        /// </summary>
        /// <param name="numbers">Tüm rulet sayıları listesi (başta en son eklenen)</param>
        /// <returns>Tahmin edilen sayı</returns>
        int PredictNextNumber(List<int> numbers);

        /// <summary>
        /// Tahminin gerçek sonuçla doğruluğunu kontrol eder ve gerekli hesaplamaları yapar
        /// </summary>
        /// <param name="predictedNumber">Tahmin edilen sayı</param>
        /// <param name="actualNumber">Gerçek sayı</param>
        /// <param name="neighbors">Tahmin edilen sayının komşuları</param>
        /// <returns>Tahmin doğru ise true, değilse false</returns>
        bool CheckPredictionAccuracy(int predictedNumber, int actualNumber, int[] neighbors);
    }
}
