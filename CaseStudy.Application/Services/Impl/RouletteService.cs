using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CaseStudy.Application.Interfaces;
using CaseStudy.Application.Models.Roulette;
using System.IO;
using System.Text.Json;

namespace CaseStudy.Application.Services.Impl
{
    public class RouletteService : IRouletteService
    {
        private readonly ILogger<RouletteService> _logger;
        private List<int> _rouletteNumbers = new List<int>();
        private int _lastPrediction = -1;
        private bool _isInitialized = false;
        private readonly string _dataFilePath = "roulette_numbers.json";

        public RouletteService(ILogger<RouletteService> logger)
        {
            _logger = logger;
            LoadNumbersFromFile();
        }

        private void LoadNumbersFromFile()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    string jsonData = File.ReadAllText(_dataFilePath);
                    var numbers = JsonSerializer.Deserialize<List<int>>(jsonData);
                    if (numbers != null && numbers.Count > 0)
                    {
                        _rouletteNumbers = numbers;
                        _isInitialized = true;
                        _logger.LogInformation($"Rulet verileri dosyadan yüklendi. Sayı adedi: {_rouletteNumbers.Count}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rulet verileri dosyadan yüklenirken hata oluştu");
            }
        }

        private void SaveNumbersToFile()
        {
            try
            {
                string jsonData = JsonSerializer.Serialize(_rouletteNumbers);
                File.WriteAllText(_dataFilePath, jsonData);
                _logger.LogInformation($"Rulet verileri dosyaya kaydedildi. Sayı adedi: {_rouletteNumbers.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rulet verileri dosyaya kaydedilirken hata oluştu");
            }
        }

        public Task<RouletteInitializeResponse> InitializeWithNumbers(List<int> initialNumbers)
        {
            try
            {
                if (initialNumbers == null || initialNumbers.Count == 0)
                {
                    _logger.LogWarning("Geçersiz başlangıç verileri.");
                    return Task.FromResult(new RouletteInitializeResponse
                    {
                        Success = false,
                        NumbersCount = 0
                    });
                }

                _logger.LogInformation($"Rulet verileri yükleniyor. Sayı adedi: {initialNumbers.Count}");
                _rouletteNumbers = new List<int>(initialNumbers);
                _isInitialized = true;
                
                // Verileri dosyaya kaydet
                SaveNumbersToFile();
                
                return Task.FromResult(new RouletteInitializeResponse
                {
                    Success = true,
                    NumbersCount = _rouletteNumbers.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rulet verileri yüklenirken hata oluştu");
                return Task.FromResult(new RouletteInitializeResponse
                {
                    Success = false,
                    NumbersCount = 0
                });
            }
        }

        public Task<RoulettePredictionResponse> AddNumberAndPredict(int newNumber)
        {
            try
            {
                _logger.LogInformation($"AddNumberAndPredict çağrıldı. Mevcut sayı adedi: {_rouletteNumbers.Count}, Başlatılmış mı: {_isInitialized}");
                
                if (_rouletteNumbers.Count == 0 || !_isInitialized)
                {
                    _logger.LogWarning("Henüz rulet verileri yüklenmemiş veya başlatılmamış.");
                    return Task.FromResult(new RoulettePredictionResponse
                    {
                        PredictedNumber = -1,
                        Numbers = new List<int>()
                    });
                }
                
                // Yeni sayıyı ekle
                _rouletteNumbers.Add(newNumber);
                _logger.LogInformation($"Yeni sayı eklendi: {newNumber}. Toplam sayı adedi: {_rouletteNumbers.Count}");
                
                // Verileri dosyaya kaydet
                SaveNumbersToFile();
                
                // Tahmin yap
                int prediction = PredictNextNumber(_rouletteNumbers);
                _lastPrediction = prediction;
                
                return Task.FromResult(new RoulettePredictionResponse
                {
                    PredictedNumber = prediction,
                    Numbers = new List<int>(_rouletteNumbers)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sayı ekleme ve tahmin sırasında hata oluştu");
                return Task.FromResult(new RoulettePredictionResponse
                {
                    PredictedNumber = -1,
                    Numbers = new List<int>(_rouletteNumbers)
                });
            }
        }

        #region Yardımcı Metodlar

        private int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count == 0)
            {
                return -1;
            }

            // 1. Sıcak sayılar stratejisi: En çok tekrar eden sayıları bul
            var hotNumbers = numbers
                .GroupBy(n => n)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();

            // 2. Soğuk sayılar stratejisi: En az tekrar eden sayıları bul
            var allPossibleNumbers = Enumerable.Range(0, 37).ToList(); // 0-36 arası rulet sayıları
            var coldNumbers = allPossibleNumbers
                .Except(numbers)
                .ToList();

            if (coldNumbers.Count == 0)
            {
                coldNumbers = allPossibleNumbers
                    .GroupBy(n => numbers.Count(pn => pn == n))
                    .OrderBy(g => g.Key)
                    .FirstOrDefault()?.ToList() ?? new List<int>();
            }

            // 3. Son sayıların tekrar etme olasılığını kontrol et
            var lastNumbers = numbers.Skip(Math.Max(0, numbers.Count - 10)).ToList();
            
            // 4. Çift/tek, kırmızı/siyah, yüksek/düşük dağılımlarını analiz et
            var oddCount = numbers.Count(n => n % 2 == 1 && n > 0);
            var evenCount = numbers.Count(n => n % 2 == 0 && n > 0);
            var zeroCount = numbers.Count(n => n == 0);
            
            var lowCount = numbers.Count(n => n > 0 && n <= 18);
            var highCount = numbers.Count(n => n > 18);

            // Kırmızı sayılar: 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36
            var redNumbers = new List<int> { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
            var redCount = numbers.Count(n => redNumbers.Contains(n));
            var blackCount = numbers.Count(n => n > 0 && !redNumbers.Contains(n));

            // 5. Sayı dizilerini analiz et (ardışık sayılar, belirli aralıklar)
            var sequences = AnalyzeSequences(numbers);

            // Tüm stratejileri bir araya getirerek ağırlıklı bir tahmin yap
            var prediction = GeneratePrediction(hotNumbers, coldNumbers, lastNumbers, 
                oddCount, evenCount, zeroCount, lowCount, highCount, redCount, blackCount, sequences);

            return prediction;
        }

        private List<List<int>> AnalyzeSequences(List<int> numbers)
        {
            var sequences = new List<List<int>>();
            
            // Son 20 sayıda tekrar eden dizileri bul
            var lastNumbers = numbers.Skip(Math.Max(0, numbers.Count - 20)).ToList();
            
            for (int i = 0; i < lastNumbers.Count - 2; i++)
            {
                var seq = new List<int> { lastNumbers[i], lastNumbers[i + 1], lastNumbers[i + 2] };
                sequences.Add(seq);
            }
            
            return sequences;
        }

        private int GeneratePrediction(
            List<int> hotNumbers, 
            List<int> coldNumbers, 
            List<int> lastNumbers,
            int oddCount,
            int evenCount,
            int zeroCount,
            int lowCount,
            int highCount,
            int redCount,
            int blackCount,
            List<List<int>> sequences)
        {
            var random = new Random();
            var candidates = new List<int>();
            
            // Sıcak sayılardan seç
            if (hotNumbers.Any())
            {
                candidates.AddRange(hotNumbers);
            }
            
            // Son çıkan sayıları değerlendir
            if (lastNumbers.Any())
            {
                // Son sayılardan birinin tekrar etme olasılığı
                if (random.NextDouble() < 0.2)
                {
                    candidates.Add(lastNumbers[random.Next(lastNumbers.Count)]);
                }
                
                // Son sayılardan sonra gelen sayıları değerlendir
                foreach (var seq in sequences)
                {
                    if (seq.Count >= 2 && lastNumbers.Count >= 2 &&
                        seq[0] == lastNumbers[lastNumbers.Count - 2] && 
                        seq[1] == lastNumbers[lastNumbers.Count - 1])
                    {
                        candidates.Add(seq[2]);
                    }
                }
            }
            
            // Çift/tek, kırmızı/siyah, yüksek/düşük dağılımlarına göre değerlendir
            if (oddCount < evenCount)
            {
                // Tek sayılar daha az çıkmış, bir tek sayı tahmin et
                candidates.AddRange(Enumerable.Range(1, 36).Where(n => n % 2 == 1));
            }
            else if (evenCount < oddCount)
            {
                // Çift sayılar daha az çıkmış, bir çift sayı tahmin et
                candidates.AddRange(Enumerable.Range(1, 36).Where(n => n % 2 == 0));
            }
            
            if (lowCount < highCount)
            {
                // Düşük sayılar daha az çıkmış, bir düşük sayı tahmin et
                candidates.AddRange(Enumerable.Range(1, 18));
            }
            else if (highCount < lowCount)
            {
                // Yüksek sayılar daha az çıkmış, bir yüksek sayı tahmin et
                candidates.AddRange(Enumerable.Range(19, 18));
            }
            
            // Kırmızı/siyah dağılımı
            var redNumbers = new List<int> { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
            if (redCount < blackCount)
            {
                // Kırmızı sayılar daha az çıkmış, bir kırmızı sayı tahmin et
                candidates.AddRange(redNumbers);
            }
            else if (blackCount < redCount)
            {
                // Siyah sayılar daha az çıkmış, bir siyah sayı tahmin et
                candidates.AddRange(Enumerable.Range(1, 36).Where(n => !redNumbers.Contains(n)));
            }
            
            // 0 çıkma olasılığını değerlendir
            if (zeroCount < (double)lastNumbers.Count / 37)
            {
                // 0 beklenen orandan daha az çıkmış
                candidates.Add(0);
            }
            
            // Soğuk sayıları değerlendir
            if (coldNumbers.Any() && random.NextDouble() < 0.3)
            {
                candidates.AddRange(coldNumbers);
            }
            
            // Tüm adaylar arasından rastgele bir tahmin seç
            if (candidates.Any())
            {
                return candidates[random.Next(candidates.Count)];
            }
            
            // Hiçbir strateji uygulanamadıysa, rastgele bir sayı döndür
            return random.Next(37); // 0-36 arası
        }

        #endregion
    }
}
