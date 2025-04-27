using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    public class BayesianAnalysisStrategy : IPredictionStrategy
    {
        private readonly Random _random;
        private readonly Dictionary<int, double> _priorProbabilities;
        private readonly Dictionary<int, Dictionary<int, int>> _likelihoodData;
        
        public string Name => "Bayesian Analiz";
        
        public BayesianAnalysisStrategy()
        {
            _random = new Random();
            
            // Öncül olasılıkları başlat (prior probabilities)
            _priorProbabilities = new Dictionary<int, double>();
            for (int i = 0; i <= 36; i++)
            {
                _priorProbabilities[i] = 1.0 / 37; // Başlangıçta eşit olasılık
            }
            
            // Olabilirlik verilerini başlat (likelihood data)
            _likelihoodData = new Dictionary<int, Dictionary<int, int>>();
            for (int i = 0; i <= 36; i++)
            {
                _likelihoodData[i] = new Dictionary<int, int>();
                for (int j = 0; j <= 36; j++)
                {
                    _likelihoodData[i][j] = 1; // Laplace düzeltmesi - tüm olayların en az 1 kez gerçekleştiğini varsay
                }
            }
        }
        
        public int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count < 5)
            {
                return _random.Next(0, 37);
            }
            
            // Bayes verisini güncelle
            UpdateBayesianData(numbers);
            
            // Son çıkan sayıyı al
            int lastNumber = numbers.First();
            
            // Bayes teoremi kullanarak her sayının olasılığını hesapla
            var posteriorProbabilities = CalculatePosteriorProbabilities(lastNumber);
            
            // En yüksek olasılığa sahip sayıları bul
            double maxProbability = posteriorProbabilities.Values.Max();
            var candidates = posteriorProbabilities
                .Where(p => Math.Abs(p.Value - maxProbability) < 0.0001) // Çok yakın olasılıkları da dahil et
                .Select(p => p.Key)
                .ToList();
            
            // Aday sayılardan birini rastgele seç
            return candidates[_random.Next(candidates.Count)];
        }
        
        public bool CheckPredictionAccuracy(int predictedNumber, int actualNumber, int[] neighbors)
        {
            // Tahmin doğruysa, öncül olasılıkları güncelle
            if (predictedNumber == actualNumber)
            {
                // Doğru tahmin durumunda bu sayının olasılığını hafifçe artır
                _priorProbabilities[actualNumber] *= 1.05;
                NormalizeProbabilities();
            }
            
            return predictedNumber == actualNumber;
        }
        
        private void UpdateBayesianData(List<int> numbers)
        {
            // Sayı çiftlerinin geçiş frekanslarını güncelle
            for (int i = 1; i < numbers.Count; i++)
            {
                int currentNumber = numbers[i];
                int previousNumber = numbers[i-1];
                
                _likelihoodData[previousNumber][currentNumber]++;
            }
            
            // Son 50 sayının frekansını analiz et ve öncül olasılıkları güncelle
            var last50Numbers = numbers.Take(Math.Min(50, numbers.Count)).ToList();
            
            // Her sayının olasılığını sayının son 50 içindeki frekansına göre hesapla
            for (int i = 0; i <= 36; i++)
            {
                int count = last50Numbers.Count(n => n == i);
                _priorProbabilities[i] = (count + 1.0) / (last50Numbers.Count + 37.0); // Laplace düzeltmesi
            }
        }
        
        private Dictionary<int, double> CalculatePosteriorProbabilities(int lastNumber)
        {
            var posteriorProbabilities = new Dictionary<int, double>();
            double totalLikelihood = 0;
            
            // Her sayı için olabilirlik (likelihood) hesapla
            for (int i = 0; i <= 36; i++)
            {
                int timesFollowed = _likelihoodData[lastNumber][i];
                int totalTransitions = _likelihoodData[lastNumber].Values.Sum();
                
                // P(Evidence|Hypothesis) - koşullu olasılık
                double likelihood = (double)timesFollowed / totalTransitions;
                
                // P(Hypothesis) - öncül olasılık
                double prior = _priorProbabilities[i];
                
                // P(Evidence|Hypothesis) * P(Hypothesis)
                double unnormalizedPosterior = likelihood * prior;
                
                posteriorProbabilities[i] = unnormalizedPosterior;
                totalLikelihood += unnormalizedPosterior;
            }
            
            // Normalize et - P(Hypothesis|Evidence) = P(Evidence|Hypothesis) * P(Hypothesis) / P(Evidence)
            if (totalLikelihood > 0)
            {
                for (int i = 0; i <= 36; i++)
                {
                    posteriorProbabilities[i] /= totalLikelihood;
                }
            }
            
            return posteriorProbabilities;
        }
        
        private void NormalizeProbabilities()
        {
            double sum = _priorProbabilities.Values.Sum();
            
            if (sum > 0)
            {
                foreach (var key in _priorProbabilities.Keys.ToList())
                {
                    _priorProbabilities[key] /= sum;
                }
            }
        }
    }
}
