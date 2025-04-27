using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    /// <summary>
    /// Yüksek/Düşük sayı dağılımı analizi stratejisi - Dengesiz yüksek/düşük dağılımlarına göre tahmin yapar
    /// </summary>
    public class HighLowDistributionStrategy : IPredictionStrategy
    {
        /// <summary>
        /// Stratejinin adı
        /// </summary>
        public string Name => "high_low_distribution";

        /// <summary>
        /// Bir sonraki sayıyı tahmin eder
        /// </summary>
        /// <param name="numbers">Tüm rulet sayıları listesi (başta en son eklenen)</param>
        /// <returns>Tahmin edilen sayı</returns>
        public int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count < 10) // Yeterli veri olmalı
            {
                return new Random(DateTime.Now.Millisecond).Next(0, 37);
            }

            var random = new Random(DateTime.Now.Millisecond);
            
            // Son 50 sayıdaki yüksek/düşük dağılımını incele
            var recentNumbers = numbers.Take(Math.Min(50, numbers.Count)).ToList();
            
            // Yüksek/düşük istatistikleri (1-18 düşük, 19-36 yüksek)
            var lowCount = recentNumbers.Count(n => n >= 1 && n <= 18); // Düşük sayılar
            var highCount = recentNumbers.Count(n => n >= 19 && n <= 36); // Yüksek sayılar
            var zeroCount = recentNumbers.Count(n => n == 0); // Sıfır sayısı
            
            // Beklenen oranlara göre dengesizlikleri hesapla
            // Sıfır hariç yüksek ve düşük sayıların dağılımı teorik olarak eşit olmalıdır
            double lowRatio = (double)lowCount / (lowCount + highCount + zeroCount);
            double highRatio = (double)highCount / (lowCount + highCount + zeroCount);
            
            List<int> candidateNumbers = new List<int>();
            
            // Düşük sayılar beklenen orandan daha az çıkmışsa
            if (lowRatio < 0.45)
            {
                // Düşük sayıları listeye ekle
                candidateNumbers.AddRange(Enumerable.Range(1, 18));
            }
            // Yüksek sayılar beklenen orandan daha az çıkmışsa
            else if (highRatio < 0.45)
            {
                // Yüksek sayıları listeye ekle
                candidateNumbers.AddRange(Enumerable.Range(19, 18));
            }
            else
            {
                // Belirgin bir dengesizlik yoksa, 0'ın gelme olasılığını değerlendir
                if (zeroCount == 0 && recentNumbers.Count >= 20)
                {
                    return 0; // 0 hiç çıkmamışsa ve yeterli veri varsa, 0 tahmin et
                }
                
                // Dengesizlik yoksa rastgele bir yüksek/düşük yaklaşımı seç
                if (random.NextDouble() < 0.5)
                {
                    candidateNumbers.AddRange(Enumerable.Range(1, 18));
                }
                else
                {
                    candidateNumbers.AddRange(Enumerable.Range(19, 18));
                }
            }
            
            // Son 5 sayıyı aday listesinden çıkar (Yakın zamanda çıkan sayıların tekrar çıkma olasılığını azalt)
            var last5 = numbers.Take(Math.Min(5, numbers.Count)).ToHashSet();
            candidateNumbers = candidateNumbers.Where(n => !last5.Contains(n)).ToList();
            
            // Eğer aday listesi boşsa (tüm adaylar son 5'te ise), rastgele bir sayı seç
            if (candidateNumbers.Count == 0)
            {
                return random.Next(0, 37);
            }
            
            // Aday listesinden rastgele bir sayı seç
            return candidateNumbers[random.Next(candidateNumbers.Count)];
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
            
            // Yüksek/düşük kategorisi doğru mu? (Daha esnek bir doğruluk tanımı)
            bool isPredictedLow = predictedNumber >= 1 && predictedNumber <= 18;
            bool isActualLow = actualNumber >= 1 && actualNumber <= 18;
            
            if ((isPredictedLow && isActualLow) || (!isPredictedLow && !isActualLow && predictedNumber != 0 && actualNumber != 0))
            {
                return true;
            }
            
            return false;
        }
    }
}
