using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    /// <summary>
    /// Soğuk sayılar stratejisi - Uzun zamandır görülmeyen veya az görülen sayıları tahmin eder
    /// </summary>
    public class ColdNumbersStrategy : IPredictionStrategy
    {
        /// <summary>
        /// Stratejinin adı
        /// </summary>
        public string Name => "cold_numbers";

        /// <summary>
        /// Bir sonraki sayıyı tahmin eder
        /// </summary>
        /// <param name="numbers">Tüm rulet sayıları listesi (başta en son eklenen)</param>
        /// <returns>Tahmin edilen sayı</returns>
        public int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count == 0)
            {
                return -1;
            }

            var random = new Random(DateTime.Now.Millisecond);
            var allPossibleNumbers = Enumerable.Range(0, 37).ToList(); // 0-36 arası rulet sayıları
            
            // En az tekrar eden sayıları bul
            var coldNumbers = allPossibleNumbers
                .Except(numbers)
                .ToList();

            // Eğer hiç görülmemiş sayı yoksa, en az görülen sayıları bul
            if (coldNumbers.Count == 0)
            {
                var frequency = new Dictionary<int, int>();
                
                foreach (var num in allPossibleNumbers)
                {
                    frequency[num] = numbers.Count(n => n == num);
                }
                
                int minFrequency = frequency.Values.Min();
                coldNumbers = frequency
                    .Where(kvp => kvp.Value == minFrequency)
                    .Select(kvp => kvp.Key)
                    .ToList();
            }

            // Soğuk sayılardan rastgele birini seç
            if (coldNumbers.Count > 0)
            {
                return coldNumbers[random.Next(coldNumbers.Count)];
            }
            else
            {
                // Beklenmeyen durum - her ihtimale karşı rastgele bir sayı döndür
                return random.Next(0, 37);
            }
        }

        /// <summary>
        /// Tahminin gerçek sonuçla doğruluğunu kontrol eder
        /// </summary>
        /// <param name="predictedNumber">Tahmin edilen sayı</param>
        /// <param name="actualNumber">Gerçek sayı</param>
        /// <param name="neighbors">Tahmin edilen sayının komşuları</param>
        /// <returns>Tahmin doğru ise true, değilse false</returns>
        public bool CheckPredictionAccuracy(int predictedNumber, int actualNumber, int[] neighbors)
        {
            // Tahmin doğrudan doğru mu?
            if (predictedNumber == actualNumber)
            {
                return true;
            }
            
            // Tahmin edilen sayının komşuları içinde mi?
            if (neighbors != null && neighbors.Contains(actualNumber))
            {
                return true;
            }
            
            return false;
        }
    }
}
