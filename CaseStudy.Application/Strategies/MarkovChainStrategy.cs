using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    public class MarkovChainStrategy : IPredictionStrategy
    {
        private readonly Random _random;
        private readonly Dictionary<int, Dictionary<int, int>> _transitionMatrix;
        
        public string Name => "Markov Zinciri Analizi";
        
        public MarkovChainStrategy()
        {
            _random = new Random();
            _transitionMatrix = new Dictionary<int, Dictionary<int, int>>();
            
            // Tüm olası sayılar için boş geçiş matrisi oluştur
            for (int i = 0; i <= 36; i++)
            {
                _transitionMatrix[i] = new Dictionary<int, int>();
                for (int j = 0; j <= 36; j++)
                {
                    _transitionMatrix[i][j] = 0;
                }
            }
        }
        
        public int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count < 5)
            {
                return _random.Next(0, 37);
            }
            
            // Geçiş matrisini güncelle
            UpdateTransitionMatrix(numbers);
            
            // Son çıkan sayıyı al
            int lastNumber = numbers.First();
            
            // Son sayıdan sonra en çok çıkan sayıyı bul
            var transitions = _transitionMatrix[lastNumber];
            
            // Hiç geçiş yoksa rastgele sayı döndür
            if (transitions.Values.Sum() == 0)
            {
                return _random.Next(0, 37);
            }
            
            // En yüksek geçiş olasılığına sahip sayıları bul
            int maxTransitions = transitions.Values.Max();
            var candidates = transitions.Where(t => t.Value == maxTransitions)
                                    .Select(t => t.Key)
                                    .ToList();
            
            // Birden fazla aday varsa, rastgele birini seç
            return candidates[_random.Next(candidates.Count)];
        }
        
        public bool CheckPredictionAccuracy(int predictedNumber, int actualNumber, int[] neighbors)
        {
            return predictedNumber == actualNumber;
        }
        
        private void UpdateTransitionMatrix(List<int> numbers)
        {
            // 1. derece Markov zinciri - her sayının bir sonraki sayıya geçiş olasılığını hesaplar
            for (int i = 1; i < numbers.Count; i++)
            {
                int currentNumber = numbers[i];
                int previousNumber = numbers[i-1];
                
                // Geçişi güncelle
                _transitionMatrix[previousNumber][currentNumber]++;
            }
            
            // Opsiyonel: 2. derece Markov Zinciri için, her iki sayının bir sonraki sayıya geçişini takip etmek istersen:
            // Bu, daha karmaşık bir matris gerektirir, örneğin: Dictionary<(int, int), Dictionary<int, int>>
        }
    }
}
