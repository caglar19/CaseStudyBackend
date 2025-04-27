using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    /// <summary>
    /// Sıcak sayılar stratejisi - Son dönemde sık görülen veya trendi artan sayıları tahmin eder
    /// </summary>
    public class HotNumbersStrategy : IPredictionStrategy
    {
        /// <summary>
        /// Stratejinin adı
        /// </summary>
        public string Name => "hot_numbers";

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

            // Eğer hot numbers bulunamazsa, en sık görülen sayıları kullan
            if (hotNumbers.Count == 0)
            {
                var allNumbersFrequency = numbers
                    .GroupBy(n => n)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => g.Key)
                    .ToList();
                
                if (allNumbersFrequency.Count > 0)
                {
                    return allNumbersFrequency[random.Next(allNumbersFrequency.Count)];
                }
                else
                {
                    return random.Next(0, 37); // 0-36 arası rastgele sayı
                }
            }

            // Sıcak sayılardan rastgele birini seç
            return hotNumbers[random.Next(hotNumbers.Count)];
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
