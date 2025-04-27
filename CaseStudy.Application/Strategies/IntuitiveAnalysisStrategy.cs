using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    public class IntuitiveAnalysisStrategy : IPredictionStrategy
    {
        private readonly Random _random;
        private readonly HashSet<int> _predictedNumbers;
        private readonly List<int> _outliers;
        private readonly Dictionary<int, double> _chaoticScores;
        
        public string Name => "Sezgisel Tahmin Analizi";
        
        public IntuitiveAnalysisStrategy()
        {
            _random = new Random();
            _predictedNumbers = new HashSet<int>();
            _outliers = new List<int>();
            _chaoticScores = new Dictionary<int, double>();
            
            // Her sayı için bir kaotik skor başlat
            for (int i = 0; i <= 36; i++)
            {
                _chaoticScores[i] = 1.0;
            }
        }
        
        public int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count < 10)
            {
                return _random.Next(0, 37);
            }
            
            // Son sayıları analiz et
            var recentNumbers = numbers.Take(50).ToList();
            
            // 1. Kaotik sistem analizi - rastgeleliğin derecesini ölç
            var entropy = CalculateEntropy(recentNumbers);
            
            // 2. Aykırı değer tespiti - beklenmedik eğilimleri ara
            DetectOutliers(recentNumbers);
            
            // 3. Son tahminleri ve gerçek sonuçları göz önünde bulundur
            var frequentPatterns = IdentifyFrequentPatterns(recentNumbers);
            
            // 4. Sezgisel tahmin: Eğer aşırı rastgele bir dizi varsa önceki tahminlerden kaçın
            if (entropy > 0.9) // Yüksek düzeyde rastgelelik
            {
                // Önceki tahmin edilmemiş sayıları tercih et
                var unusedNumbers = Enumerable.Range(0, 37)
                    .Where(n => !_predictedNumbers.Contains(n))
                    .ToList();
                
                if (unusedNumbers.Count > 0)
                {
                    // Daha önce tahmin edilmemiş bir sayı seç
                    return unusedNumbers[_random.Next(unusedNumbers.Count)];
                }
            }
            
            // 5. Aykırı değerler tespit edildiyse, bunlar arasından seç
            if (_outliers.Count > 0 && _random.NextDouble() < 0.4)
            {
                return _outliers[_random.Next(_outliers.Count)];
            }
            
            // 6. Sık tekrarlayan desenleri kullan
            if (frequentPatterns.Count > 0 && _random.NextDouble() < 0.6)
            {
                return frequentPatterns[_random.Next(frequentPatterns.Count)];
            }
            
            // 7. Kaotik sistemde en yüksek skorlu sayıları değerlendir
            var topChaoticNumbers = _chaoticScores
                .OrderByDescending(kv => kv.Value)
                .Take(5)
                .Select(kv => kv.Key)
                .ToList();
            
            if (topChaoticNumbers.Count > 0)
            {
                return topChaoticNumbers[_random.Next(topChaoticNumbers.Count)];
            }
            
            // 8. Son çare: Rastgele bir sayı seç, ama son çıkan sayılardan kaçın
            var lastFive = recentNumbers.Take(5).ToHashSet();
            int candidate;
            
            do
            {
                candidate = _random.Next(0, 37);
            } while (lastFive.Contains(candidate));
            
            return candidate;
        }
        
        public bool CheckPredictionAccuracy(int predictedNumber, int actualNumber, int[] neighbors)
        {
            // Tahmin edilen sayıyı kaydet
            _predictedNumbers.Add(predictedNumber);
            
            // Tahmin doğruysa, bu sayının kaotik skorunu artır
            if (predictedNumber == actualNumber)
            {
                _chaoticScores[actualNumber] *= 1.5;
                NormalizeChaoticScores();
            }
            // Tahmin yanlışsa ama komşu sayılardansa, yine de kısmen başarılı kabul et
            else if (neighbors != null && neighbors.Contains(predictedNumber))
            {
                _chaoticScores[actualNumber] *= 1.2;
                NormalizeChaoticScores();
            }
            
            return predictedNumber == actualNumber;
        }
        
        private double CalculateEntropy(List<int> numbers)
        {
            // Shannon entropy hesaplama - serinin rastgeleliğini ölçer
            var frequencies = new Dictionary<int, int>();
            for (int i = 0; i <= 36; i++) frequencies[i] = 0;
            
            foreach (var num in numbers)
            {
                frequencies[num]++;
            }
            
            double entropy = 0;
            double logBase = Math.Log(37); // 37 olası sonuç (0-36)
            
            foreach (var freq in frequencies.Values)
            {
                if (freq > 0)
                {
                    double p = (double)freq / numbers.Count;
                    entropy -= p * Math.Log(p) / logBase;
                }
            }
            
            return entropy;
        }
        
        private void DetectOutliers(List<int> numbers)
        {
            // Seriyi alt gruplara bölerek yerel aykırılıkları tespit et
            _outliers.Clear();
            
            // Son çıkan sayıların frekansını bul
            var frequencies = new Dictionary<int, int>();
            for (int i = 0; i <= 36; i++) frequencies[i] = 0;
            
            foreach (var num in numbers)
            {
                frequencies[num]++;
            }
            
            // Frekans ortalaması ve standart sapması
            double mean = frequencies.Values.Average();
            double variance = frequencies.Values.Select(v => Math.Pow(v - mean, 2)).Average();
            double stdDev = Math.Sqrt(variance);
            
            // Z-skor hesaplayarak aykırı değerleri bul (z-skor > 2 olanlar)
            foreach (var kvp in frequencies)
            {
                double zScore = Math.Abs(kvp.Value - mean) / stdDev;
                if (zScore > 2.0)
                {
                    _outliers.Add(kvp.Key);
                }
            }
            
            // Sayı gruplarında dizi kırılmalarını ara
            for (int i = 0; i < numbers.Count - 2; i++)
            {
                int a = numbers[i];
                int b = numbers[i+1];
                int c = numbers[i+2];
                
                // Herhangi bir örüntü kırılması var mı?
                if ((a < b && b > c) || (a > b && b < c))
                {
                    _outliers.Add(b);
                }
            }
        }
        
        private List<int> IdentifyFrequentPatterns(List<int> numbers)
        {
            var patterns = new List<int>();
            
            // Son 3'lü grupları analiz et
            for (int i = 0; i < numbers.Count - 2; i++)
            {
                var triplet = new[] { numbers[i], numbers[i+1], numbers[i+2] };
                
                // Benzer 3'lü grupları ara
                for (int j = i + 3; j < numbers.Count - 2; j++)
                {
                    if (numbers[j] == triplet[0] && numbers[j+1] == triplet[1])
                    {
                        // Benzer bir dizi bulundu, bir sonraki numarayı kaydet
                        patterns.Add(numbers[j+2]);
                        break;
                    }
                }
            }
            
            return patterns.Distinct().ToList();
        }
        
        private void NormalizeChaoticScores()
        {
            double sum = _chaoticScores.Values.Sum();
            
            if (sum > 0)
            {
                foreach (var key in _chaoticScores.Keys.ToList())
                {
                    _chaoticScores[key] /= sum;
                    _chaoticScores[key] *= 37; // 37 sayı için norm
                }
            }
        }
    }
}
