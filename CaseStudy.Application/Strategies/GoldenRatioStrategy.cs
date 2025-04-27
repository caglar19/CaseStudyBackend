using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    public class GoldenRatioStrategy : IPredictionStrategy
    {
        private readonly Random _random;
        private readonly double _goldenRatio = 1.618033988749895; // Altın oran
        private readonly int[] _fibonacciNumbers = { 1, 2, 3, 5, 8, 13, 21, 34 }; // Rulet çarkında kullanılabilecek Fibonacci sayıları
        
        public string Name => "Altın Oran Analizi";
        
        public GoldenRatioStrategy()
        {
            _random = new Random();
        }
        
        public int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count < 5)
            {
                return _random.Next(0, 37);
            }
            
            // Son sayılardan altın oran bazlı örüntüler bul
            var lastNumbers = numbers.Take(10).ToList();
            
            // 1. Fibonacci ilişkisi - son sayıların Fibonacci dizisine olan yakınlığını kontrol et
            var fibonacciCandidate = FindFibonacciRelation(lastNumbers);
            if (fibonacciCandidate >= 0)
            {
                return fibonacciCandidate;
            }
            
            // 2. Altın oran ilişkisi - son sayının altın orana göre sonraki değerini hesapla
            var goldenRatioCandidate = CalculateGoldenRatioNumber(lastNumbers);
            if (goldenRatioCandidate >= 0 && goldenRatioCandidate <= 36)
            {
                return goldenRatioCandidate;
            }
            
            // 3. Geometrik sıralama - çark üzerindeki sayıların geometrik dağılımına göre tahmin yap
            var geometricCandidate = PredictGeometricPattern(lastNumbers);
            if (geometricCandidate >= 0)
            {
                return geometricCandidate;
            }
            
            // Hiçbir örüntü bulunamazsa, son sayıların Fibonacci bazlı bir uzantısını dene
            int lastNumber = lastNumbers.First();
            
            // Son sayıyla Fibonacci sayılarının en yakın modüler ilişkisini bul
            foreach (var fib in _fibonacciNumbers)
            {
                int candidate = (lastNumber + fib) % 37;
                if (candidate >= 0 && candidate <= 36)
                {
                    return candidate;
                }
            }
            
            return _random.Next(0, 37);
        }
        
        public bool CheckPredictionAccuracy(int predictedNumber, int actualNumber, int[] neighbors)
        {
            return predictedNumber == actualNumber;
        }
        
        private int FindFibonacciRelation(List<int> numbers)
        {
            // Son sayılardan Fibonacci dizisini andıran ilişkileri ara
            if (numbers.Count < 3) return -1;
            
            for (int i = 0; i < numbers.Count - 2; i++)
            {
                int a = numbers[i+2];
                int b = numbers[i+1];
                int c = numbers[i];
                
                // Fibonacci benzeri bir dizi varsa (a ≈ b + c)
                if (Math.Abs(a - (b + c)) <= 2 || (a + b) % 37 == c)
                {
                    // Diziyi devam ettir
                    return (b + c) % 37;
                }
            }
            
            return -1;
        }
        
        private int CalculateGoldenRatioNumber(List<int> numbers)
        {
            // Son iki sayının altın oran ilişkisine göre bir sonraki sayıyı hesapla
            if (numbers.Count < 2) return -1;
            
            int lastNumber = numbers[0];
            int secondLastNumber = numbers[1];
            
            // Altın oran bazlı hesaplama
            double nextValue = lastNumber * _goldenRatio;
            int nextNumber = (int)Math.Round(nextValue) % 37;
            
            // Alternatif: İki sayı arasındaki farkın altın oran katı kadar ilerlet
            double diff = Math.Abs(lastNumber - secondLastNumber);
            int altNextNumber = (int)(lastNumber + diff * _goldenRatio) % 37;
            
            // Hangisi daha uygun bir değer veriyorsa onu seç
            if (nextNumber >= 0 && nextNumber <= 36)
            {
                return nextNumber;
            }
            else if (altNextNumber >= 0 && altNextNumber <= 36)
            {
                return altNextNumber;
            }
            
            return -1;
        }
        
        private int PredictGeometricPattern(List<int> numbers)
        {
            // Rulet çarkı üzerindeki geometrik dağılıma göre tahmin
            if (numbers.Count < 3) return -1;
            
            // Son üç sayının çark üzerindeki açısal dağılımını hesapla
            double angle1 = (numbers[0] * 360.0 / 37.0) % 360;
            double angle2 = (numbers[1] * 360.0 / 37.0) % 360;
            double angle3 = (numbers[2] * 360.0 / 37.0) % 360;
            
            // Açıların altın orana göre dağılıp dağılmadığını kontrol et
            double angleDiff1 = Math.Abs(angle1 - angle2);
            double angleDiff2 = Math.Abs(angle2 - angle3);
            
            if (Math.Abs(angleDiff1 / angleDiff2 - _goldenRatio) < 0.2)
            {
                // Altın oran bazlı bir dağılım varsa, bir sonraki açıyı tahmin et
                double nextAngle = (angle1 + angleDiff1 * _goldenRatio) % 360;
                int nextNumber = (int)Math.Round(nextAngle * 37.0 / 360.0) % 37;
                
                return nextNumber;
            }
            
            return -1;
        }
    }
}
