using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    public class MotionVectorStrategy : IPredictionStrategy
    {
        private readonly Random _random;
        private readonly int[] _wheelSequence = new int[] {
            0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10, 5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
        };
        
        public string Name => "Hareket Vektörü Analizi";
        
        public MotionVectorStrategy()
        {
            _random = new Random();
        }
        
        public int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count < 5)
            {
                return _random.Next(0, 37);
            }
            
            // Son 5 sayıyı al
            var lastFive = numbers.Take(5).ToList();
            
            // Bu sayıların çarktaki pozisyonlarını bul
            var positions = lastFive.Select(n => Array.IndexOf(_wheelSequence, n)).ToList();
            
            // Geçersiz pozisyon varsa (sayı çarkta yoksa), rastgele tahmin yap
            if (positions.Any(p => p == -1))
            {
                return _random.Next(0, 37);
            }
            
            // Hareket vektörünü hesapla (son iki hareket arasındaki ortalama)
            int vector = CalculateAverageVector(positions);
            
            // Vektörün yönünü belirle (saat yönünde veya saat yönü tersine)
            bool clockwise = IsClockwiseMotion(positions);
            
            // Son pozisyona vektörü ekleyerek sonraki pozisyonu tahmin et
            int lastPosition = positions[0];
            int predictedPosition;
            
            if (clockwise)
            {
                predictedPosition = (lastPosition + vector) % _wheelSequence.Length;
            }
            else
            {
                predictedPosition = (lastPosition - vector + _wheelSequence.Length) % _wheelSequence.Length;
            }
            
            // Tahmin edilen pozisyondaki sayıyı döndür
            return _wheelSequence[predictedPosition];
        }
        
        public bool CheckPredictionAccuracy(int predictedNumber, int actualNumber, int[] neighbors)
        {
            return predictedNumber == actualNumber;
        }
        
        private int CalculateAverageVector(List<int> positions)
        {
            // Son 4 hareketin vektörlerini hesapla
            var vectors = new List<int>();
            
            for (int i = 0; i < positions.Count - 1; i++)
            {
                int current = positions[i];
                int next = positions[i+1];
                
                // İki yöndeki hareket mesafelerini hesapla
                int clockwiseDistance = (current - next + _wheelSequence.Length) % _wheelSequence.Length;
                int counterClockwiseDistance = (next - current + _wheelSequence.Length) % _wheelSequence.Length;
                
                // En kısa mesafeyi seç
                int distance = Math.Min(clockwiseDistance, counterClockwiseDistance);
                vectors.Add(distance);
            }
            
            // Vektörlerin ortalamasını hesapla
            return (int)Math.Round(vectors.Average());
        }
        
        private bool IsClockwiseMotion(List<int> positions)
        {
            // Son 2-3 hareketin yönünü belirleyerek baskın yönü seç
            int clockwiseCount = 0;
            int counterClockwiseCount = 0;
            
            for (int i = 0; i < positions.Count - 1; i++)
            {
                int current = positions[i];
                int next = positions[i+1];
                
                // İki yöndeki hareket mesafelerini hesapla
                int clockwiseDistance = (current - next + _wheelSequence.Length) % _wheelSequence.Length;
                int counterClockwiseDistance = (next - current + _wheelSequence.Length) % _wheelSequence.Length;
                
                if (clockwiseDistance < counterClockwiseDistance)
                {
                    clockwiseCount++;
                }
                else
                {
                    counterClockwiseCount++;
                }
            }
            
            // Hangi yön daha baskın
            return clockwiseCount >= counterClockwiseCount;
        }
    }
}
