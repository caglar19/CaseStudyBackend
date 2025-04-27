using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    public class HybridPredictionStrategy : IPredictionStrategy
    {
        private readonly Random _random;
        private readonly Dictionary<string, double> _strategyWeights;
        private readonly Dictionary<string, int> _strategySuccess;
        private readonly Dictionary<string, int> _strategyUsage;
        
        public string Name => "Hibrit Tahmin Stratejisi";
        
        // Bu meta-strateji diğer stratejileri dinamik olarak değerlendirir
        // StrategyManager'ın bir benzeri gibi çalışır fakat sadece kendi içinde
        
        public HybridPredictionStrategy()
        {
            _random = new Random();
            _strategyWeights = new Dictionary<string, double>();
            _strategySuccess = new Dictionary<string, int>();
            _strategyUsage = new Dictionary<string, int>();
            
            // Takip etmek istediğimiz olası strateji adları
            string[] strategyNames = new string[] {
                "Sıcak Sayılar", "Soğuk Sayılar", "Tek/Çift Dağılımı", "Yüksek/Düşük Dağılımı",
                "Kırmızı/Siyah Dağılımı", "Dizi Analizi", "Tekrarlanma Aralıkları", 
                "Son Sayılara Ceza", "Monte Carlo Simülasyonu", "Makine Öğrenmesi Entegrasyonu",
                "Sektör Bazlı Analiz", "Markov Zinciri Analizi", "Trigram Analiz",
                "Bayesian Analiz", "Zamansal Analiz"
            };
            
            // Tüm olası stratejileri başlangıç ağırlıklarıyla tanımla
            foreach (var strategy in strategyNames)
            {
                _strategyWeights[strategy] = 1.0; // Başlangıçta eşit ağırlık
                _strategySuccess[strategy] = 0;
                _strategyUsage[strategy] = 0;
            }
        }
        
        public int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count < 10)
            {
                return _random.Next(0, 37);
            }
            
            // Hibrit strateji kendi içinde her bir stratejiyi simüle eder
            var predictions = new Dictionary<int, double>();
            for (int i = 0; i <= 36; i++)
            {
                predictions[i] = 0;
            }
            
            // 1. Sıcak Sayılar Simülasyonu
            var hotNumbers = GetHotNumbers(numbers);
            foreach (var number in hotNumbers)
            {
                predictions[number] += _strategyWeights["Sıcak Sayılar"];
            }
            
            // 2. Soğuk Sayılar Simülasyonu
            var coldNumbers = GetColdNumbers(numbers);
            foreach (var number in coldNumbers)
            {
                predictions[number] += _strategyWeights["Soğuk Sayılar"];
            }
            
            // 3. Tek/Çift Dağılımı Simülasyonu
            bool predictOdd = ShouldPredictOdd(numbers);
            for (int i = 1; i <= 36; i++)
            {
                if ((predictOdd && i % 2 == 1) || (!predictOdd && i % 2 == 0))
                {
                    predictions[i] += _strategyWeights["Tek/Çift Dağılımı"];
                }
            }
            
            // 4. Yüksek/Düşük Dağılımı Simülasyonu
            bool predictHigh = ShouldPredictHigh(numbers);
            for (int i = 1; i <= 36; i++)
            {
                if ((predictHigh && i > 18) || (!predictHigh && i <= 18))
                {
                    predictions[i] += _strategyWeights["Yüksek/Düşük Dağılımı"];
                }
            }
            
            // 5. Kırmızı/Siyah Dağılımı Simülasyonu
            bool predictRed = ShouldPredictRed(numbers);
            var redNumbers = new List<int> { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
            for (int i = 1; i <= 36; i++)
            {
                if ((predictRed && redNumbers.Contains(i)) || (!predictRed && !redNumbers.Contains(i)))
                {
                    predictions[i] += _strategyWeights["Kırmızı/Siyah Dağılımı"];
                }
            }
            
            // 6. Dizi Analizi Simülasyonu
            var sequenceNumber = SimulateSequenceAnalysis(numbers);
            if (sequenceNumber >= 0)
            {
                predictions[sequenceNumber] += _strategyWeights["Dizi Analizi"];
            }
            
            // 7. Tekrarlanma Aralıkları Simülasyonu
            var recurrenceNumber = SimulateRecurrenceIntervals(numbers);
            if (recurrenceNumber >= 0)
            {
                predictions[recurrenceNumber] += _strategyWeights["Tekrarlanma Aralıkları"];
            }
            
            // 8. Son Sayılara Ceza Simülasyonu (son çıkan sayıların olasılığını azalt)
            var recentNumbers = numbers.Take(5).ToList();
            foreach (var number in recentNumbers)
            {
                if (predictions.ContainsKey(number))
                {
                    predictions[number] -= _strategyWeights["Son Sayılara Ceza"];
                }
            }
            
            // 9. Monte Carlo Simülasyonu (basit simülasyon)
            var monteCarloNumber = SimulateMonteCarlo(numbers);
            if (monteCarloNumber >= 0)
            {
                predictions[monteCarloNumber] += _strategyWeights["Monte Carlo Simülasyonu"];
            }
            
            // 10. Diğer strateji simülasyonları burada eklenebilir
            
            // En yüksek birleşik skora sahip sayıyı bul
            double maxScore = predictions.Values.Max();
            var candidates = predictions
                .Where(p => Math.Abs(p.Value - maxScore) < 0.001)
                .Select(p => p.Key)
                .ToList();
            
            return candidates[_random.Next(candidates.Count)];
        }
        
        public bool CheckPredictionAccuracy(int predictedNumber, int actualNumber, int[] neighbors)
        {
            // Tahmin doğruysa, hangi stratejilerin bu tahmini desteklediğini kontrol et
            foreach (var strategy in _strategyWeights.Keys.ToList())
            {
                _strategyUsage[strategy]++;
                
                bool strategySupported = false;
                
                // Genel bir doğrulama yaparak, hangi stratejilerin bu sonucu desteklediğini belirle
                switch (strategy)
                {
                    case "Sıcak Sayılar":
                        strategySupported = GetHotNumbers(new List<int> { actualNumber }).Contains(actualNumber);
                        break;
                    case "Soğuk Sayılar":
                        strategySupported = GetColdNumbers(new List<int> { actualNumber }).Contains(actualNumber);
                        break;
                    // Diğer strateji doğrulamaları buraya eklenebilir
                }
                
                if (strategySupported)
                {
                    _strategySuccess[strategy]++;
                }
                
                // Stratejinin başarı oranını güncelle
                if (_strategyUsage[strategy] > 0)
                {
                    double successRate = (double)_strategySuccess[strategy] / _strategyUsage[strategy];
                    
                    // Stratejinin ağırlığını dinamik olarak güncelle
                    _strategyWeights[strategy] = Math.Max(0.1, Math.Min(5.0, successRate * 3.0));
                }
            }
            
            return predictedNumber == actualNumber;
        }
        
        #region Strateji Simülasyonları
        
        private List<int> GetHotNumbers(List<int> numbers)
        {
            // Sadece basit bir simülasyon - son 30 sayı içinde en çok tekrar eden 5 sayıyı bul
            var limitedList = numbers.Take(Math.Min(30, numbers.Count)).ToList();
            
            var frequencies = new Dictionary<int, int>();
            for (int i = 0; i <= 36; i++) frequencies[i] = 0;
            
            foreach (var num in limitedList)
            {
                frequencies[num]++;
            }
            
            return frequencies
                .OrderByDescending(kv => kv.Value)
                .Take(5)
                .Select(kv => kv.Key)
                .ToList();
        }
        
        private List<int> GetColdNumbers(List<int> numbers)
        {
            // Basit simülasyon - son 30 sayı içinde hiç çıkmayan veya en az çıkan 5 sayıyı bul
            var limitedList = numbers.Take(Math.Min(30, numbers.Count)).ToList();
            
            var frequencies = new Dictionary<int, int>();
            for (int i = 0; i <= 36; i++) frequencies[i] = 0;
            
            foreach (var num in limitedList)
            {
                frequencies[num]++;
            }
            
            return frequencies
                .OrderBy(kv => kv.Value)
                .Take(5)
                .Select(kv => kv.Key)
                .ToList();
        }
        
        private bool ShouldPredictOdd(List<int> numbers)
        {
            // Son 20 sayıyı al
            var limitedList = numbers.Take(Math.Min(20, numbers.Count))
                                    .Where(n => n > 0) // 0 hariç
                                    .ToList();
            
            int oddCount = limitedList.Count(n => n % 2 == 1);
            int evenCount = limitedList.Count - oddCount;
            
            // Eğer son 20'de çift sayılar daha fazla çıkmışsa, dengeye gelsin diye tek tahmin et
            return evenCount > oddCount;
        }
        
        private bool ShouldPredictHigh(List<int> numbers)
        {
            // Son 20 sayıyı al
            var limitedList = numbers.Take(Math.Min(20, numbers.Count))
                                    .Where(n => n > 0) // 0 hariç
                                    .ToList();
            
            int highCount = limitedList.Count(n => n > 18);
            int lowCount = limitedList.Count - highCount;
            
            // Eğer son 20'de düşük sayılar daha fazla çıkmışsa, dengeye gelsin diye yüksek tahmin et
            return lowCount > highCount;
        }
        
        private bool ShouldPredictRed(List<int> numbers)
        {
            // Kırmızı sayılar
            var redNumbers = new List<int> { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
            
            // Son 20 sayıyı al
            var limitedList = numbers.Take(Math.Min(20, numbers.Count))
                                    .Where(n => n > 0) // 0 hariç
                                    .ToList();
            
            int redCount = limitedList.Count(n => redNumbers.Contains(n));
            int blackCount = limitedList.Count - redCount;
            
            // Eğer son 20'de siyah sayılar daha fazla çıkmışsa, dengeye gelsin diye kırmızı tahmin et
            return blackCount > redCount;
        }
        
        private int SimulateSequenceAnalysis(List<int> numbers)
        {
            // Çok basit dizi analizi simülasyonu - son 3 sayıdaki artış/azalış trendine bakarak tahmin yap
            if (numbers.Count < 3) return -1;
            
            int last = numbers[0];
            int secondLast = numbers[1];
            int thirdLast = numbers[2];
            
            // Artan dizi: Önceki iki sayı artıyorsa, artış devam etsin
            if (secondLast > thirdLast && last > secondLast)
            {
                int difference = last - secondLast;
                return Math.Min(36, last + difference);
            }
            
            // Azalan dizi: Önceki iki sayı azalıyorsa, azalış devam etsin
            if (secondLast < thirdLast && last < secondLast)
            {
                int difference = secondLast - last;
                return Math.Max(0, last - difference);
            }
            
            return -1; // Belirgin bir dizi bulunamadı
        }
        
        private int SimulateRecurrenceIntervals(List<int> numbers)
        {
            // Son 50 sayıyı al
            var last50 = numbers.Take(Math.Min(50, numbers.Count)).ToList();
            
            // Her sayının son görülme indeksini tut
            var lastSeen = new Dictionary<int, int>();
            for (int i = 0; i <= 36; i++)
            {
                lastSeen[i] = -1;
            }
            
            // Sayıların son görüldüğü pozisyonları belirle
            for (int i = 0; i < last50.Count; i++)
            {
                int num = last50[i];
                if (lastSeen[num] == -1)
                {
                    lastSeen[num] = i;
                }
            }
            
            // En uzun süredir görülmeyen sayıyı bul
            return lastSeen.OrderByDescending(kv => kv.Value == -1 ? 999 : kv.Value)
                          .First().Key;
        }
        
        private int SimulateMonteCarlo(List<int> numbers)
        {
            // Basit Monte Carlo simülasyonu - mevcut dağılıma dayalı rastgele seçim
            var last30 = numbers.Take(Math.Min(30, numbers.Count)).ToList();
            
            // Frekansları hesapla
            var frequencies = new Dictionary<int, int>();
            for (int i = 0; i <= 36; i++) frequencies[i] = 1; // Laplace düzeltmesi
            
            foreach (var num in last30)
            {
                frequencies[num]++;
            }
            
            // Toplam frekansı hesapla
            double totalFrequency = frequencies.Values.Sum();
            
            // Rastgele bir değer seç (ağırlıklı)
            double randomValue = _random.NextDouble() * totalFrequency;
            double cumulativeFrequency = 0;
            
            foreach (var kvp in frequencies)
            {
                cumulativeFrequency += kvp.Value;
                if (cumulativeFrequency >= randomValue)
                {
                    return kvp.Key;
                }
            }
            
            return _random.Next(0, 37); // Buraya ulaşılmamalı
        }
        
        #endregion
    }
}
