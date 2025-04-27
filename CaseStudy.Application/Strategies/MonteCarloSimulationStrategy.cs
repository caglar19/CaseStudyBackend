using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    public class MonteCarloSimulationStrategy : IPredictionStrategy
    {
        private readonly Random _random;
        private const int SIMULATION_COUNT = 5000; // Simülasyon sayısı
        
        public string Name => "Monte Carlo Simülasyonu";
        
        public MonteCarloSimulationStrategy()
        {
            _random = new Random();
        }
        
        public int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count < 10)
            {
                // Yeterli veri yoksa rastgele bir sayı döndür (0-36 arası)
                return _random.Next(0, 37);
            }
            
            // Son çıkan sayıların frekansını hesapla
            var frequencies = CalculateFrequencies(numbers);
            
            // Monte Carlo simülasyonu yap
            var simulationResults = RunMonteCarloSimulation(numbers, frequencies);
            
            // En yüksek olasılıklı sayıyı bul
            int predictedNumber = simulationResults
                .OrderByDescending(kv => kv.Value)
                .First().Key;
                
            return predictedNumber;
        }
        
        public bool CheckPredictionAccuracy(int predictedNumber, int actualNumber, int[] neighbors)
        {
            // Direkt karşılaştırma: tahmin edilen ve gerçek sayı aynı mı?
            return predictedNumber == actualNumber;
        }
        
        private Dictionary<int, int> CalculateFrequencies(List<int> numbers)
        {
            var frequencies = new Dictionary<int, int>();
            
            // 0-36 arası tüm sayılar için başlangıç değeri ata
            for (int i = 0; i <= 36; i++)
            {
                frequencies[i] = 0;
            }
            
            // Sayıların frekanslarını hesapla (ağırlıklı olarak son sayılar daha önemli)
            for (int i = 0; i < numbers.Count; i++)
            {
                int number = numbers[i];
                int weight = Math.Max(1, numbers.Count - i); // Son sayılar daha ağırlıklı
                frequencies[number] += weight;
            }
            
            return frequencies;
        }
        
        private Dictionary<int, double> RunMonteCarloSimulation(List<int> numbers, Dictionary<int, int> frequencies)
        {
            var results = new Dictionary<int, double>();
            var totalWeightSum = frequencies.Values.Sum();
            
            // 0-36 arası tüm sayılar için başlangıç değeri ata
            for (int i = 0; i <= 36; i++)
            {
                results[i] = 0;
            }
            
            // Monte Carlo simülasyonu yap
            for (int sim = 0; sim < SIMULATION_COUNT; sim++)
            {
                // Rulet simülasyonu
                double randValue = _random.NextDouble() * totalWeightSum;
                double cumSum = 0;
                
                foreach (var kvp in frequencies)
                {
                    cumSum += kvp.Value;
                    if (cumSum >= randValue)
                    {
                        results[kvp.Key]++;
                        break;
                    }
                }
            }
            
            // Sonuçları normalleştir (0-1 arası)
            foreach (var key in results.Keys.ToList())
            {
                results[key] /= SIMULATION_COUNT;
            }
            
            return results;
        }
    }
}
