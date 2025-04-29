using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    /// <summary>
    /// Son Çıkan Sayılara Ceza Stratejisi - Son çıkan sayıların tekrar gelmesini azaltmak için cezalandırma yaklaşımı uygular
    /// </summary>
    public class RecentNumbersPenaltyStrategy : IPredictionStrategy
    {
        /// <summary>
        /// Stratejinin adı
        /// </summary>
        public string Name => "recent_numbers_penalty";

        /// <summary>
        /// Bir sonraki sayıyı tahmin eder
        /// </summary>
        /// <param name="numbers">Tüm rulet sayıları listesi (başta en son eklenen)</param>
        /// <returns>Tahmin edilen sayı</returns>
        public int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count < 5)
            {
                return new Random(DateTime.Now.Millisecond).Next(0, 37);
            }

            var random = new Random(DateTime.Now.Millisecond);
            
            // Tüm mümkün sayılar ve ağırlıkları
            var candidateWeights = new Dictionary<int, int>();
            
            // Tüm olası sayılar için başlangıç değeri
            for (int i = 0; i <= 36; i++)
            {
                candidateWeights[i] = 10; // Başlangıçta herkese iyi bir ağırlık ver
            }
            
            // Son çıkan sayıların listesi
            var recentNumbers = numbers.Take(500).ToList();
            
            // Son 10 sayıya ceza uygula - en son çıkanlara daha fazla ceza
            for (int i = 0; i < recentNumbers.Count; i++)
            {
                int num = recentNumbers[i];
                int penalty = 500 - i; // Son çıkan sayıya 10, bir öncekine 9... ceza uygula
                
                if (candidateWeights.ContainsKey(num))
                {
                    candidateWeights[num] = Math.Max(1, candidateWeights[num] - penalty);
                }
            }
            
            // Son 50 sayıdaki frekansları hesapla
            var last50 = numbers.Take(Math.Min(50, numbers.Count));
            var frequencyCount = new Dictionary<int, int>();
            
            foreach (var num in last50)
            {
                if (frequencyCount.ContainsKey(num))
                {
                    frequencyCount[num]++;
                }
                else
                {
                    frequencyCount[num] = 1;
                }
            }
            
            // Hiç çıkmamış veya az çıkmış sayılara bonus ver
            for (int i = 0; i <= 36; i++)
            {
                if (!frequencyCount.ContainsKey(i) || frequencyCount[i] < 2)
                {
                    candidateWeights[i] += 5; // Hiç çıkmamış veya az çıkmış sayılara bonus
                }
            }
            
            // Dağılım dengesizliklerini kontrol et
            
            // Kırmızı/siyah dağılımı
            var redNumbers = new List<int> { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
            var redCount = last50.Count(n => redNumbers.Contains(n));
            var blackCount = last50.Count(n => n > 0 && !redNumbers.Contains(n));
            
            // Eğer belirgin bir dengesizlik varsa, az çıkan renge bonus ver
            if (redCount < blackCount * 0.7)
            {
                // Kırmızılar az çıkmış
                foreach (var num in redNumbers)
                {
                    candidateWeights[num] += 3;
                }
            }
            else if (blackCount < redCount * 0.7)
            {
                // Siyahlar az çıkmış
                for (int i = 1; i <= 36; i++)
                {
                    if (!redNumbers.Contains(i))
                    {
                        candidateWeights[i] += 3;
                    }
                }
            }
            
            // Tek/çift dağılımı
            var oddCount = last50.Count(n => n % 2 == 1 && n > 0);
            var evenCount = last50.Count(n => n % 2 == 0 && n > 0);
            
            // Eğer belirgin bir dengesizlik varsa, az çıkana bonus ver
            if (oddCount < evenCount * 0.7)
            {
                // Tek sayılar az çıkmış
                for (int i = 1; i <= 35; i += 2)
                {
                    candidateWeights[i] += 3;
                }
            }
            else if (evenCount < oddCount * 0.7)
            {
                // Çift sayılar az çıkmış
                for (int i = 2; i <= 36; i += 2)
                {
                    candidateWeights[i] += 3;
                }
            }
            
            // 0 için özel durum
            var zeroCount = last50.Count(n => n == 0);
            if (zeroCount == 0)
            {
                // 0 hiç çıkmamış, ona bonus ver
                candidateWeights[0] += 5;
            }
            else if (zeroCount >= 3)
            {
                // 0 çok çıkmış, ceza ver
                candidateWeights[0] = Math.Max(1, candidateWeights[0] - 5);
            }
            
            // En yüksek ağırlıklı sayıları bul
            int maxWeight = candidateWeights.Values.Max();
            var topCandidates = candidateWeights
                .Where(kvp => kvp.Value >= maxWeight * 0.8)
                .Select(kvp => kvp.Key)
                .ToList();
            
            // Eğer belirgin bir aday yoksa, rastgele bir sayı döndür
            if (topCandidates.Count == 0)
            {
                return random.Next(0, 37);
            }
            
            // En yüksek ağırlıklı adaylardan rastgele birini seç
            return topCandidates[random.Next(topCandidates.Count)];
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
