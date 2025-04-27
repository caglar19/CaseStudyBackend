using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    public class TemporalAnalysisStrategy : IPredictionStrategy
    {
        private readonly Random _random;
        private readonly Dictionary<int, Dictionary<int, int>> _hourlyPatterns;
        private readonly Dictionary<int, Dictionary<int, int>> _dailyPatterns;
        
        public string Name => "Zamansal Analiz";
        
        public TemporalAnalysisStrategy()
        {
            _random = new Random();
            
            // Saatlik paternleri tut (0-23 arası saatler)
            _hourlyPatterns = new Dictionary<int, Dictionary<int, int>>();
            for (int hour = 0; hour < 24; hour++)
            {
                _hourlyPatterns[hour] = new Dictionary<int, int>();
                for (int num = 0; num <= 36; num++)
                {
                    _hourlyPatterns[hour][num] = 0;
                }
            }
            
            // Günlük paternleri tut (1-7 arası günler, 1=Pazartesi)
            _dailyPatterns = new Dictionary<int, Dictionary<int, int>>();
            for (int day = 1; day <= 7; day++)
            {
                _dailyPatterns[day] = new Dictionary<int, int>();
                for (int num = 0; num <= 36; num++)
                {
                    _dailyPatterns[day][num] = 0;
                }
            }
        }
        
        public int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count < 10)
            {
                return _random.Next(0, 37);
            }
            
            // Şu anki zamanı al
            DateTime now = DateTime.Now;
            int currentHour = now.Hour;
            int currentDay = (int)now.DayOfWeek == 0 ? 7 : (int)now.DayOfWeek; // 1=Pazartesi ... 7=Pazar
            
            // Saatlik ve günlük paternleri analiz et
            var hourlyProbabilities = CalculateHourlyProbabilities(currentHour);
            var dailyProbabilities = CalculateDailyProbabilities(currentDay);
            
            // İki olasılık setini birleştir
            var combinedProbabilities = new Dictionary<int, double>();
            for (int i = 0; i <= 36; i++)
            {
                // Saatlik ve günlük olasılıkları ağırlıklı olarak birleştir
                combinedProbabilities[i] = hourlyProbabilities[i] * 0.6 + dailyProbabilities[i] * 0.4;
            }
            
            // En yüksek olasılığa sahip sayıları bul
            double maxProbability = combinedProbabilities.Values.Max();
            var candidates = combinedProbabilities
                .Where(p => Math.Abs(p.Value - maxProbability) < 0.0001)
                .Select(p => p.Key)
                .ToList();
            
            // Aday sayılardan birini rastgele seç
            return candidates.Count > 0 ? candidates[_random.Next(candidates.Count)] : _random.Next(0, 37);
        }
        
        public bool CheckPredictionAccuracy(int predictedNumber, int actualNumber, int[] neighbors)
        {
            // Yeni gelen sayıyı zamansal verilere ekle
            DateTime now = DateTime.Now;
            int hour = now.Hour;
            int day = (int)now.DayOfWeek == 0 ? 7 : (int)now.DayOfWeek;
            
            // Saatlik paterni güncelle
            if (!_hourlyPatterns[hour].ContainsKey(actualNumber))
            {
                _hourlyPatterns[hour][actualNumber] = 0;
            }
            _hourlyPatterns[hour][actualNumber]++;
            
            // Günlük paterni güncelle
            if (!_dailyPatterns[day].ContainsKey(actualNumber))
            {
                _dailyPatterns[day][actualNumber] = 0;
            }
            _dailyPatterns[day][actualNumber]++;
            
            return predictedNumber == actualNumber;
        }
        
        private Dictionary<int, double> CalculateHourlyProbabilities(int currentHour)
        {
            var probabilities = new Dictionary<int, double>();
            double totalOccurrences = _hourlyPatterns[currentHour].Values.Sum();
            
            if (totalOccurrences == 0)
            {
                // Veri yoksa eşit olasılık
                for (int i = 0; i <= 36; i++)
                {
                    probabilities[i] = 1.0 / 37;
                }
            }
            else
            {
                for (int i = 0; i <= 36; i++)
                {
                    int occurrences = _hourlyPatterns[currentHour].ContainsKey(i) ? _hourlyPatterns[currentHour][i] : 0;
                    
                    // Laplace düzeltmesi - veri yoksa bile küçük bir olasılık ata
                    probabilities[i] = (occurrences + 0.1) / (totalOccurrences + 3.7);
                }
            }
            
            return probabilities;
        }
        
        private Dictionary<int, double> CalculateDailyProbabilities(int currentDay)
        {
            var probabilities = new Dictionary<int, double>();
            double totalOccurrences = _dailyPatterns[currentDay].Values.Sum();
            
            if (totalOccurrences == 0)
            {
                // Veri yoksa eşit olasılık
                for (int i = 0; i <= 36; i++)
                {
                    probabilities[i] = 1.0 / 37;
                }
            }
            else
            {
                for (int i = 0; i <= 36; i++)
                {
                    int occurrences = _dailyPatterns[currentDay].ContainsKey(i) ? _dailyPatterns[currentDay][i] : 0;
                    
                    // Laplace düzeltmesi - veri yoksa bile küçük bir olasılık ata
                    probabilities[i] = (occurrences + 0.1) / (totalOccurrences + 3.7);
                }
            }
            
            return probabilities;
        }
    }
}
