using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    /// <summary>
    /// Tek/Çift dağılımı analizi stratejisi - Dengesiz tek/çift dağılımlarına göre tahmin yapar
    /// </summary>
    public class OddEvenDistributionStrategy : IPredictionStrategy
    {
        /// <summary>
        /// Stratejinin adı
        /// </summary>
        public string Name => "odd_even_distribution";

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
            
            // Son 50 sayıdaki tek/çift dağılımını incele
            var recentNumbers = numbers.Take(Math.Min(500, numbers.Count)).ToList();
            
            // Tek/çift istatistikleri
            var oddCount = recentNumbers.Count(n => n % 2 == 1 && n > 0); // Tek sayılar
            var evenCount = recentNumbers.Count(n => n % 2 == 0 && n > 0); // Çift sayılar
            var zeroCount = recentNumbers.Count(n => n == 0); // Sıfır sayısı
            
            // Beklenen oranlara göre dengesizlikleri hesapla
            // Sıfır hariç tek ve çiftlerin dağılımı teorik olarak eşit olmalıdır
            double oddRatio = (double)oddCount / (oddCount + evenCount + zeroCount);
            double evenRatio = (double)evenCount / (oddCount + evenCount + zeroCount);
            
            List<int> candidateNumbers = new List<int>();
            
            // Tek sayılar beklenen orandan daha az çıkmışsa
            if (oddRatio < 0.45)
            {
                // Tek sayıları listeye ekle
                candidateNumbers.AddRange(Enumerable.Range(1, 36).Where(n => n % 2 == 1));
            }
            // Çift sayılar beklenen orandan daha az çıkmışsa
            else if (evenRatio < 0.45)
            {
                // Çift sayıları listeye ekle
                candidateNumbers.AddRange(Enumerable.Range(1, 36).Where(n => n % 2 == 0));
            }
            else
            {
                // Belirgin bir dengesizlik yoksa, 0'ın gelme olasılığını değerlendir
                if (zeroCount == 0 && recentNumbers.Count >= 20)
                {
                    return 0; // 0 hiç çıkmamışsa ve yeterli veri varsa, 0 tahmin et
                }
                
                // Dengesizlik yoksa rastgele bir tek/çift yaklaşımı seç
                if (random.NextDouble() < 0.5)
                {
                    candidateNumbers.AddRange(Enumerable.Range(1, 36).Where(n => n % 2 == 1));
                }
                else
                {
                    candidateNumbers.AddRange(Enumerable.Range(1, 36).Where(n => n % 2 == 0));
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
            
            // NOT: Tek/Çift kategorisi kontrolü kaldırıldı.
            // Sadece direkt sayı eşleşmesi veya komşu sayı kontrolü yapılıyor.
            
            return false;
        }
    }
}
