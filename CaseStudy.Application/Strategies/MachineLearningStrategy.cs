using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    public class MachineLearningStrategy : IPredictionStrategy
    {
        private readonly Random _random;
        private readonly int[] _weights; // Her sayı için ağırlıklar (öğrenme ağırlıkları)
        private const double LEARNING_RATE = 0.2; // Öğrenme hızı
        private const int HISTORY_WINDOW = 10; // Dikkate alınacak son sayı adedi
        
        public string Name => "Makine Öğrenmesi Entegrasyonu";
        
        public MachineLearningStrategy()
        {
            _random = new Random();
            
            // 0-36 arası sayılar için başlangıç ağırlıklarını oluştur
            _weights = new int[37];
            for (int i = 0; i <= 36; i++)
            {
                _weights[i] = 100; // Başlangıçta tüm sayıların ağırlığı eşit
            }
        }
        
        public int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count == 0)
            {
                // Veri yoksa rastgele bir sayı döndür
                return _random.Next(0, 37);
            }

            // Son çıkan sayıların örüntülerini analiz et
            AnalyzePatterns(numbers);
            
            // Sayı çiftlerini analiz et (örneğin: X çıkınca genelde Y çıkıyor mu?)
            AnalyzeNumberPairs(numbers);
            
            // Belirli intervallerde sayı tekrarlarını kontrol et 
            AnalyzeIntervals(numbers);
            
            // Tüm sayıların ağırlıklı olasılıklarını hesapla
            var probabilities = CalculateWeightedProbabilities();

            // En yüksek olasılığa sahip sayıyı seç (birden fazla eşit olasılıklı sayı varsa rastgele seç)
            var maxProbability = probabilities.Max(p => p.Value);
            var candidates = probabilities.Where(p => p.Value == maxProbability).Select(p => p.Key).ToList();
            
            int predictedNumber = candidates[_random.Next(candidates.Count)];
            return predictedNumber;
        }
        
        public bool CheckPredictionAccuracy(int predictedNumber, int actualNumber, int[] neighbors)
        {
            // Tahmin doğruluğunu kontrol et ve ağırlıkları güncelle
            UpdateWeights(predictedNumber, actualNumber);
            
            return predictedNumber == actualNumber;
        }
        
        private void AnalyzePatterns(List<int> numbers)
        {
            // Son N sayıyı incele
            var recentNumbers = numbers.Take(Math.Min(HISTORY_WINDOW, numbers.Count)).ToList();
            
            // Çift/tek dağılımı
            int oddCount = recentNumbers.Count(n => n > 0 && n % 2 == 1);
            int evenCount = recentNumbers.Count(n => n > 0 && n % 2 == 0);
            
            // Eğer belirgin bir eğilim varsa, ağırlıkları güncelle
            if (oddCount > evenCount * 1.5) // Tek sayılar daha sık çıkıyorsa
            {
                // Tek sayıların ağırlığını azalt (çünkü dengeye gelme eğilimi olabilir)
                for (int i = 1; i <= 35; i += 2)
                {
                    _weights[i] = Math.Max(50, _weights[i] - 10);
                }
                
                // Çift sayıların ağırlığını artır
                for (int i = 2; i <= 36; i += 2)
                {
                    _weights[i] = Math.Min(150, _weights[i] + 10);
                }
            }
            else if (evenCount > oddCount * 1.5) // Çift sayılar daha sık çıkıyorsa
            {
                // Çift sayıların ağırlığını azalt
                for (int i = 2; i <= 36; i += 2)
                {
                    _weights[i] = Math.Max(50, _weights[i] - 10);
                }
                
                // Tek sayıların ağırlığını artır
                for (int i = 1; i <= 35; i += 2)
                {
                    _weights[i] = Math.Min(150, _weights[i] + 10);
                }
            }
            
            // Benzer şekilde düşük/yüksek sayı dağılımını da incele
            int lowCount = recentNumbers.Count(n => n > 0 && n <= 18);
            int highCount = recentNumbers.Count(n => n > 18);
            
            // Eğilime göre ağırlıkları güncelle
            if (lowCount > highCount * 1.5)
            {
                // Düşük sayıların ağırlığını azalt
                for (int i = 1; i <= 18; i++)
                {
                    _weights[i] = Math.Max(50, _weights[i] - 10);
                }
                
                // Yüksek sayıların ağırlığını artır
                for (int i = 19; i <= 36; i++)
                {
                    _weights[i] = Math.Min(150, _weights[i] + 10);
                }
            }
            else if (highCount > lowCount * 1.5)
            {
                // Yüksek sayıların ağırlığını azalt
                for (int i = 19; i <= 36; i++)
                {
                    _weights[i] = Math.Max(50, _weights[i] - 10);
                }
                
                // Düşük sayıların ağırlığını artır
                for (int i = 1; i <= 18; i++)
                {
                    _weights[i] = Math.Min(150, _weights[i] + 10);
                }
            }
        }
        
        private void AnalyzeNumberPairs(List<int> numbers)
        {
            if (numbers.Count < 2)
                return;
                
            // Son sayılar arasındaki ilişkileri analiz et
            for (int i = 1; i < Math.Min(HISTORY_WINDOW, numbers.Count); i++)
            {
                int current = numbers[i];
                int previous = numbers[i-1];
                
                // Belirli sayı çiftleri arasında ilişki varsa ağırlıkları güncelle
                if (previous == current)
                {
                    // Aynı sayı tekrar gelmiş, bu sayının tekrar gelme olasılığını azalt
                    _weights[current] = Math.Max(50, _weights[current] - 20);
                }
                else if (Math.Abs(previous - current) <= 3 || previous + current == 36)
                {
                    // Yakın sayılar gelmiş veya toplamları 36 ise, benzer sayıların gelme olasılığını artır
                    int potentialNext = (current + previous) % 37;
                    _weights[potentialNext] = Math.Min(150, _weights[potentialNext] + 15);
                }
            }
        }
        
        private void AnalyzeIntervals(List<int> numbers)
        {
            // Son 100 sayı içinde hangi sayılar uzun süredir gelmedi?
            var last100 = numbers.Take(Math.Min(100, numbers.Count)).ToList();
            
            // Her sayının son görülme pozisyonunu bul
            var lastSeen = new Dictionary<int, int>();
            for (int i = 0; i <= 36; i++)
            {
                lastSeen[i] = -1;
            }
            
            for (int i = 0; i < last100.Count; i++)
            {
                int num = last100[i];
                if (lastSeen[num] == -1)
                {
                    lastSeen[num] = i;
                }
            }
            
            // Uzun süredir gelmeyen sayıların ağırlığını artır
            foreach (var kvp in lastSeen)
            {
                int number = kvp.Key;
                int position = kvp.Value;
                
                if (position == -1) // Hiç görülmemiş
                {
                    _weights[number] = Math.Min(200, _weights[number] + 25);
                }
                else if (position > 50) // Uzun süredir görülmemiş
                {
                    _weights[number] = Math.Min(175, _weights[number] + 15);
                }
                else if (position < 10) // Yakın zamanda görülmüş
                {
                    _weights[number] = Math.Max(50, _weights[number] - 10);
                }
            }
        }
        
        private Dictionary<int, double> CalculateWeightedProbabilities()
        {
            var probabilities = new Dictionary<int, double>();
            double totalWeight = _weights.Sum();
            
            for (int i = 0; i <= 36; i++)
            {
                probabilities[i] = _weights[i] / totalWeight;
            }
            
            return probabilities;
        }
        
        private void UpdateWeights(int predictedNumber, int actualNumber)
        {
            // Tahmin doğruysa, o sayının ağırlığını artır
            if (predictedNumber == actualNumber)
            {
                _weights[actualNumber] = Math.Min(200, _weights[actualNumber] + 20);
            }
            else
            {
                // Tahmin yanlışsa, tahmin edilen sayının ağırlığını azalt
                _weights[predictedNumber] = Math.Max(50, _weights[predictedNumber] - 10);
                
                // Gerçekte gelen sayının ağırlığını artır
                _weights[actualNumber] = Math.Min(200, _weights[actualNumber] + 10);
            }
        }
    }
}
