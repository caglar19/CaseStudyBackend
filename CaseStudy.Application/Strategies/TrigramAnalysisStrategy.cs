using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    public class TrigramAnalysisStrategy : IPredictionStrategy
    {
        private readonly Random _random;
        private readonly Dictionary<string, Dictionary<int, int>> _trigramPatterns;
        
        public string Name => "Trigram Analiz";
        
        public TrigramAnalysisStrategy()
        {
            _random = new Random();
            _trigramPatterns = new Dictionary<string, Dictionary<int, int>>();
        }
        
        public int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count < 5)
            {
                return _random.Next(0, 37);
            }
            
            // Trigram veritabanını güncelle
            UpdateTrigramPatterns(numbers);
            
            // Son üç sayıyı al
            var lastThree = numbers.Take(3).ToArray();
            string trigramKey = $"{lastThree[2]}-{lastThree[1]}-{lastThree[0]}";
            
            // Bu trigram daha önce görülmüş mü kontrol et
            if (!_trigramPatterns.ContainsKey(trigramKey) || _trigramPatterns[trigramKey].Count == 0)
            {
                // Son iki sayı ile de kontrol et (bigram)
                string bigramKey = $"{lastThree[1]}-{lastThree[0]}";
                
                if (!_trigramPatterns.ContainsKey(bigramKey) || _trigramPatterns[bigramKey].Count == 0)
                {
                    // Hiçbir desen bulunamadıysa rastgele sayı döndür
                    return _random.Next(0, 37);
                }
                
                // Bigram deseni için en güçlü adayları bul
                var bigramFollowers = _trigramPatterns[bigramKey];
                int maxOccurrences = bigramFollowers.Values.Max();
                var candidates = bigramFollowers.Where(f => f.Value == maxOccurrences)
                                             .Select(f => f.Key)
                                             .ToList();
                
                return candidates[_random.Next(candidates.Count)];
            }
            
            // Trigram deseni için en güçlü adayları bul
            var trigramFollowers = _trigramPatterns[trigramKey];
            int maxTrigramOccurrences = trigramFollowers.Values.Max();
            var trigramCandidates = trigramFollowers.Where(f => f.Value == maxTrigramOccurrences)
                                               .Select(f => f.Key)
                                               .ToList();
            
            return trigramCandidates[_random.Next(trigramCandidates.Count)];
        }
        
        public bool CheckPredictionAccuracy(int predictedNumber, int actualNumber, int[] neighbors)
        {
            return predictedNumber == actualNumber;
        }
        
        private void UpdateTrigramPatterns(List<int> numbers)
        {
            // Üçlü (trigram) ve ikili (bigram) desenleri takip et
            for (int i = 3; i < numbers.Count; i++)
            {
                // Üç önceki, iki önceki ve bir önceki sayılar
                int thirdLast = numbers[i];
                int secondLast = numbers[i-1];
                int firstLast = numbers[i-2];
                int current = numbers[i-3];
                
                // Trigram key oluştur
                string trigramKey = $"{thirdLast}-{secondLast}-{firstLast}";
                
                // Bigram key oluştur (son iki sayı)
                string bigramKey = $"{secondLast}-{firstLast}";
                
                // Trigram için takip eden sayıyı kaydet
                if (!_trigramPatterns.ContainsKey(trigramKey))
                {
                    _trigramPatterns[trigramKey] = new Dictionary<int, int>();
                }
                
                if (!_trigramPatterns[trigramKey].ContainsKey(current))
                {
                    _trigramPatterns[trigramKey][current] = 0;
                }
                
                _trigramPatterns[trigramKey][current]++;
                
                // Bigram için takip eden sayıyı kaydet
                if (!_trigramPatterns.ContainsKey(bigramKey))
                {
                    _trigramPatterns[bigramKey] = new Dictionary<int, int>();
                }
                
                if (!_trigramPatterns[bigramKey].ContainsKey(current))
                {
                    _trigramPatterns[bigramKey][current] = 0;
                }
                
                _trigramPatterns[bigramKey][current]++;
            }
        }
    }
}
