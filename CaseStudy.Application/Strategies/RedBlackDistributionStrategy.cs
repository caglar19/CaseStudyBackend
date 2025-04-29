using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    /// <summary>
    /// Kırmızı/Siyah dağılımı analizi stratejisi - Dengesiz kırmızı/siyah dağılımlarına göre tahmin yapar
    /// </summary>
    public class RedBlackDistributionStrategy : IPredictionStrategy
    {
        // Kırmızı sayılar listesi
        private readonly List<int> _redNumbers = new List<int> { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };

        /// <summary>
        /// Stratejinin adı
        /// </summary>
        public string Name => "red_black_distribution";

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
            
            // Son 50 sayıdaki kırmızı/siyah dağılımını incele
            var recentNumbers = numbers.Take(Math.Min(500, numbers.Count)).ToList();
            
            // Kırmızı/siyah istatistikleri
            var redCount = recentNumbers.Count(n => _redNumbers.Contains(n)); // Kırmızı sayılar
            var blackCount = recentNumbers.Count(n => n > 0 && !_redNumbers.Contains(n)); // Siyah sayılar
            var zeroCount = recentNumbers.Count(n => n == 0); // Sıfır sayısı
            
            // Beklenen oranlara göre dengesizlikleri hesapla
            // Sıfır hariç kırmızı ve siyah sayıların dağılımı teorik olarak eşit olmalıdır
            double redRatio = (double)redCount / (redCount + blackCount + zeroCount);
            double blackRatio = (double)blackCount / (redCount + blackCount + zeroCount);
            
            List<int> candidateNumbers = new List<int>();
            
            // Kırmızı sayılar beklenen orandan daha az çıkmışsa
            if (redRatio < 0.45)
            {
                // Kırmızı sayıları listeye ekle
                candidateNumbers.AddRange(_redNumbers);
            }
            // Siyah sayılar beklenen orandan daha az çıkmışsa
            else if (blackRatio < 0.45)
            {
                // Siyah sayıları listeye ekle
                candidateNumbers.AddRange(Enumerable.Range(1, 36).Where(n => !_redNumbers.Contains(n)));
            }
            else
            {
                // Belirgin bir dengesizlik yoksa, 0'ın gelme olasılığını değerlendir
                if (zeroCount == 0 && recentNumbers.Count >= 20)
                {
                    return 0; // 0 hiç çıkmamışsa ve yeterli veri varsa, 0 tahmin et
                }
                
                // Dengesizlik yoksa rastgele bir kırmızı/siyah yaklaşımı seç
                if (random.NextDouble() < 0.5)
                {
                    candidateNumbers.AddRange(_redNumbers);
                }
                else
                {
                    candidateNumbers.AddRange(Enumerable.Range(1, 36).Where(n => !_redNumbers.Contains(n)));
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
            
            // NOT: Kırmızı/Siyah kategorisi kontrolü kaldırıldı.
            // Sadece direkt sayı eşleşmesi veya komşu sayı kontrolü yapılıyor.
            
            return false;
        }
    }
}
