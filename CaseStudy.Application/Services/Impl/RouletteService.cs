using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CaseStudy.Application.Interfaces;
using CaseStudy.Application.Models.Roulette;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;

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
                
                // Yeni sayıyı listenin başına ekle
                _rouletteNumbers.Insert(0, newNumber);
                _logger.LogInformation($"Yeni sayı listenin başına eklendi: {newNumber}. Toplam sayı adedi: {_rouletteNumbers.Count}");
                
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

        public Task<RouletteExtractNumbersResponse> ExtractNumbersFromHtml(string htmlContent)
        {
            try
            {
                _logger.LogInformation("HTML içeriğinden rulet sayıları çıkarılıyor");
                
                if (string.IsNullOrEmpty(htmlContent))
                {
                    _logger.LogWarning("Geçersiz HTML içeriği");
                    return Task.FromResult(new RouletteExtractNumbersResponse
                    {
                        Success = false,
                        ErrorMessage = "HTML içeriği boş veya geçersiz"
                    });
                }
                
                // HTML içeriğinden rulet sayılarını çıkar
                var numbers = new List<int>();
                
                // data-role="number-X" şeklindeki değerleri bul
                // Örnek: data-role="number-16"
                var regex = new Regex(@"data-role=""number-([0-9]+)""");
                var matches = regex.Matches(htmlContent);
                
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1 && int.TryParse(match.Groups[1].Value, out int number))
                    {
                        numbers.Add(number);
                    }
                }
                
                // Alternatif olarak <span class="value--dd5c7">X</span> şeklindeki değerleri de kontrol et
                var valueRegex = new Regex(@"<span class=""value--[a-z0-9]+"">(\d+)</span>");
                var valueMatches = valueRegex.Matches(htmlContent);
                
                foreach (Match match in valueMatches)
                {
                    if (match.Groups.Count > 1 && int.TryParse(match.Groups[1].Value, out int number))
                    {
                        // Eğer sayı daha önce eklenmemişse ekle
                        if (!numbers.Contains(number))
                        {
                            numbers.Add(number);
                        }
                    }
                }
                
                if (numbers.Count == 0)
                {
                    _logger.LogWarning("HTML içeriğinde rulet sayısı bulunamadı");
                    return Task.FromResult(new RouletteExtractNumbersResponse
                    {
                        Success = false,
                        ErrorMessage = "HTML içeriğinde rulet sayısı bulunamadı"
                    });
                }
                
                _logger.LogInformation($"HTML içeriğinden {numbers.Count} adet rulet sayısı çıkarıldı");
                
                return Task.FromResult(new RouletteExtractNumbersResponse
                {
                    Success = true,
                    Numbers = numbers,
                    NumbersCount = numbers.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTML içeriğinden rulet sayıları çıkarılırken hata oluştu");
                return Task.FromResult(new RouletteExtractNumbersResponse
                {
                    Success = false,
                    ErrorMessage = $"HTML içeriğinden rulet sayıları çıkarılırken hata oluştu: {ex.Message}"
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
            // Not: Sayılar listenin başında olduğu için ilk 15 sayıyı alıyoruz (daha fazla veri için)
            var lastNumbers = numbers.Take(15).ToList();
            
            // 4. Çift/tek, kırmızı/siyah, yüksek/düşük dağılımlarını analiz et
            // Son 30 sayıya daha fazla ağırlık ver
            var recentNumbers = numbers.Take(Math.Min(30, numbers.Count)).ToList();
            var olderNumbers = numbers.Skip(30).ToList();
            
            // Son sayıların ağırlığı daha fazla olsun
            var oddCount = recentNumbers.Count(n => n % 2 == 1 && n > 0) * 2 + olderNumbers.Count(n => n % 2 == 1 && n > 0);
            var evenCount = recentNumbers.Count(n => n % 2 == 0 && n > 0) * 2 + olderNumbers.Count(n => n % 2 == 0 && n > 0);
            var zeroCount = recentNumbers.Count(n => n == 0) * 2 + olderNumbers.Count(n => n == 0);
            
            var lowCount = recentNumbers.Count(n => n > 0 && n <= 18) * 2 + olderNumbers.Count(n => n > 0 && n <= 18);
            var highCount = recentNumbers.Count(n => n > 18) * 2 + olderNumbers.Count(n => n > 18);

            // Kırmızı sayılar: 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36
            var redNumbers = new List<int> { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
            var redCount = recentNumbers.Count(n => redNumbers.Contains(n)) * 2 + olderNumbers.Count(n => redNumbers.Contains(n));
            var blackCount = recentNumbers.Count(n => n > 0 && !redNumbers.Contains(n)) * 2 + olderNumbers.Count(n => n > 0 && !redNumbers.Contains(n));

            // 5. Sayı dizilerini analiz et (ardışık sayılar, belirli aralıklar)
            var sequences = AnalyzeSequences(numbers);

            // 6. Komşu sayıların frekansını analiz et (9 sağ ve 9 sol komşu)
            var neighborFrequency = AnalyzeNeighborFrequency(numbers);

            // Tüm stratejileri bir araya getirerek ağırlıklı bir tahmin yap
            var prediction = GeneratePrediction(hotNumbers, coldNumbers, lastNumbers, 
                oddCount, evenCount, zeroCount, lowCount, highCount, redCount, blackCount, 
                sequences, neighborFrequency);

            return prediction;
        }

        private List<List<int>> AnalyzeSequences(List<int> numbers)
        {
            var result = new List<List<int>>();
            if (numbers == null || numbers.Count < 3)
            {
                return result;
            }

            // Sayılar listenin başında olduğu için ilk 25 sayıyı al (daha kapsamlı analiz için)
            var lastNumbers = numbers.Take(Math.Min(25, numbers.Count)).ToList();
            
            // 3'lü dizileri bul
            for (int i = 0; i < lastNumbers.Count - 2; i++)
            {
                var seq = new List<int> { lastNumbers[i], lastNumbers[i + 1], lastNumbers[i + 2] };
                
                // Bu dizi daha önce var mı kontrol et
                if (!result.Any(s => s.SequenceEqual(seq)))
                {
                    // Bu dizi başka yerde tekrar ediyor mu?
                    for (int j = i + 3; j < lastNumbers.Count - 2; j++)
                    {
                        if (lastNumbers[j] == seq[0] && 
                            j + 1 < lastNumbers.Count && lastNumbers[j + 1] == seq[1] &&
                            j + 2 < lastNumbers.Count && lastNumbers[j + 2] == seq[2])
                        {
                            result.Add(seq);
                            break;
                        }
                    }
                }
            }
            
            // 2'li dizileri de analiz et (daha kısa örüntüler için)
            for (int i = 0; i < lastNumbers.Count - 1; i++)
            {
                var seq = new List<int> { lastNumbers[i], lastNumbers[i + 1], -1 }; // -1 placeholder for prediction
                
                // Bu dizi başka yerde tekrar ediyor mu?
                for (int j = i + 2; j < lastNumbers.Count - 1; j++)
                {
                    if (lastNumbers[j] == seq[0] && j + 1 < lastNumbers.Count && lastNumbers[j + 1] == seq[1])
                    {
                        // Eğer bu ikili dizi sonrasında bir sayı varsa, onu tahmin olarak ekle
                        if (j + 2 < lastNumbers.Count)
                        {
                            seq[2] = lastNumbers[j + 2];
                            if (!result.Any(s => s[0] == seq[0] && s[1] == seq[1] && s[2] == seq[2]))
                            {
                                result.Add(new List<int>(seq));
                            }
                        }
                        break;
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 9 sağ ve 9 sol komşu sayıların frekansını analiz eder
        /// </summary>
        /// <param name="numbers">Analiz edilecek sayı listesi</param>
        /// <returns>Her sayının komşularının frekansını içeren sözlük</returns>
        private Dictionary<int, Dictionary<string, int>> AnalyzeNeighborFrequency(List<int> numbers)
        {
            // Her sayı için komşu sayıların frekansını tutacak sözlük
            var neighborFrequency = new Dictionary<int, Dictionary<string, int>>();
            
            // Tüm olası rulet sayıları için sözlük oluştur
            for (int i = 0; i <= 36; i++)
            {
                neighborFrequency[i] = new Dictionary<string, int>
                {
                    { "right", 0 }, // 9 sağ komşu frekansı
                    { "left", 0 }   // 9 sol komşu frekansı
                };
            }
            
            if (numbers == null || numbers.Count < 2)
            {
                return neighborFrequency;
            }
            
            // Son 100 sayıyı analiz et (daha fazla veri için)
            var recentNumbers = numbers.Take(Math.Min(100, numbers.Count)).ToList();
            
            // Her sayı için, sonraki sayının 9 sağ veya 9 sol komşusu olup olmadığını kontrol et
            for (int i = 0; i < recentNumbers.Count - 1; i++)
            {
                int currentNumber = recentNumbers[i];
                int nextNumber = recentNumbers[i + 1];
                
                // 9 sağ komşu kontrolü
                int rightNeighbor = (currentNumber + 9) % 37; // 37'ye göre mod al (0-36 arası sayılar)
                if (nextNumber == rightNeighbor)
                {
                    neighborFrequency[currentNumber]["right"]++;
                }
                
                // 9 sol komşu kontrolü
                int leftNeighbor = (currentNumber + 28) % 37; // (currentNumber - 9 + 37) % 37 ile aynı
                if (nextNumber == leftNeighbor)
                {
                    neighborFrequency[currentNumber]["left"]++;
                }
            }
            
            return neighborFrequency;
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
            List<List<int>> sequences,
            Dictionary<int, Dictionary<string, int>> neighborFrequency)
        {
            var random = new Random(DateTime.Now.Millisecond); // Daha iyi rastgelelik için seed değerini değiştir
            var candidates = new List<int>();
            var weightedCandidates = new Dictionary<int, int>(); // Sayı ve ağırlık çifti
            
            // Tüm olası sayıları başlangıçta düşük ağırlıkla ekle (çeşitlilik için)
            for (int i = 0; i <= 36; i++)
            {
                weightedCandidates[i] = 1;
            }
            
            // Rastgele bir faktör belirle (her çağrıda farklı stratejilere ağırlık vermek için)
            double randomFactor = random.NextDouble();
            
            // Sıcak sayıları değerlendir (rastgele faktöre göre ağırlık ver)
            if (hotNumbers.Any())
            {
                int hotWeight = randomFactor < 0.5 ? 2 : 4; // Bazen daha fazla, bazen daha az ağırlık ver
                foreach (var num in hotNumbers)
                {
                    AddOrUpdateCandidate(weightedCandidates, num, hotWeight);
                }
            }
            
            // Dizi analizini değerlendir - sayılar listenin başında olduğu için ilk sayıları kontrol et
            if (sequences.Any() && lastNumbers.Count >= 2)
            {
                int seqMatchCount = 0;
                foreach (var seq in sequences)
                {
                    // Tam eşleşme kontrolü - son iki sayı bir dizinin başlangıcı mı?
                    if (seq.Count >= 3 && 
                        lastNumbers.Count >= 2 &&
                        lastNumbers[0] == seq[1] && 
                        lastNumbers[1] == seq[0])
                    {
                        // Dizi eşleşmesi bulundu, bu durumda seq[2] tahmin edilir
                        // Daha yüksek ağırlık ver çünkü bu güçlü bir örüntü
                        AddOrUpdateCandidate(weightedCandidates, seq[2], 6 + random.Next(1, 4)); // Rastgele ek ağırlık
                        seqMatchCount++;
                    }
                    
                    // Kısmi eşleşme kontrolü - son sayı bir dizinin parçası mı?
                    if (seq.Count >= 3 && lastNumbers.Count >= 1)
                    {
                        for (int i = 0; i < seq.Count - 1; i++)
                        {
                            if (lastNumbers[0] == seq[i])
                            {
                                // Kısmi eşleşme bulundu, bu durumda seq[i+1] tahmin edilir
                                AddOrUpdateCandidate(weightedCandidates, seq[i+1], 3 + random.Next(0, 3)); // Rastgele ek ağırlık
                                seqMatchCount++;
                                break;
                            }
                        }
                    }
                    
                    // Çok fazla dizi eşleşmesi varsa, bazılarını rastgele seç (çeşitlilik için)
                    if (seqMatchCount > 3 && random.NextDouble() > 0.7)
                    {
                        break;
                    }
                }
            }
            
            // Çift/tek dağılımını değerlendir
            double oddRatio = (double)oddCount / (oddCount + evenCount + zeroCount);
            double evenRatio = (double)evenCount / (oddCount + evenCount + zeroCount);
            double zeroRatio = (double)zeroCount / (oddCount + evenCount + zeroCount);
            
            // Beklenen oranlar: tek ~0.486, çift ~0.486, sıfır ~0.027
            // Rastgele bir strateji seç
            double strategyRandom = random.NextDouble();
            
            if (strategyRandom < 0.33) // Tek/çift stratejisi
            {
                if (oddRatio < 0.45)
                {
                    // Tek sayılar beklenen orandan daha az çıkmış
                    foreach (var num in Enumerable.Range(1, 36).Where(n => n % 2 == 1))
                    {
                        AddOrUpdateCandidate(weightedCandidates, num, 1 + random.Next(0, 2));
                    }
                }
                else if (evenRatio < 0.45)
                {
                    // Çift sayılar beklenen orandan daha az çıkmış
                    foreach (var num in Enumerable.Range(1, 36).Where(n => n % 2 == 0))
                    {
                        AddOrUpdateCandidate(weightedCandidates, num, 1 + random.Next(0, 2));
                    }
                }
            }
            else if (strategyRandom < 0.66) // Yüksek/düşük stratejisi
            {
                double lowRatio = (double)lowCount / (lowCount + highCount);
                if (lowRatio < 0.45)
                {
                    // Düşük sayılar az çıkmış
                    foreach (var num in Enumerable.Range(1, 18))
                    {
                        AddOrUpdateCandidate(weightedCandidates, num, 1 + random.Next(0, 2));
                    }
                }
                else
                {
                    // Yüksek sayılar az çıkmış
                    foreach (var num in Enumerable.Range(19, 18))
                    {
                        AddOrUpdateCandidate(weightedCandidates, num, 1 + random.Next(0, 2));
                    }
                }
            }
            else // Kırmızı/siyah stratejisi
            {
                // Kırmızı sayılar: 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36
                var redNumbers = new List<int> { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
                double redRatio = (double)redCount / (redCount + blackCount);
                
                if (redRatio < 0.45)
                {
                    // Kırmızı sayılar az çıkmış
                    foreach (var num in redNumbers)
                    {
                        AddOrUpdateCandidate(weightedCandidates, num, 1 + random.Next(0, 2));
                    }
                }
                else
                {
                    // Siyah sayılar az çıkmış
                    foreach (var num in Enumerable.Range(1, 36).Where(n => !redNumbers.Contains(n)))
                    {
                        AddOrUpdateCandidate(weightedCandidates, num, 1 + random.Next(0, 2));
                    }
                }
            }
            
            // 0 için özel durum
            if (zeroRatio < 0.02 && random.NextDouble() < 0.4)
            {
                AddOrUpdateCandidate(weightedCandidates, 0, 2 + random.Next(0, 3));
            }
            
            // Soğuk sayıları değerlendir (rastgele faktöre göre)
            if (coldNumbers.Any() && random.NextDouble() < 0.4)
            {
                // Rastgele birkaç soğuk sayı seç (hepsini değil)
                var selectedCold = coldNumbers.OrderBy(x => random.Next()).Take(Math.Min(5, coldNumbers.Count)).ToList();
                foreach (var num in selectedCold)
                {
                    AddOrUpdateCandidate(weightedCandidates, num, 1 + random.Next(1, 3));
                }
            }
            
            // Son 5 sayıyı kontrol et - bunların tekrar gelme olasılığı düşük
            if (lastNumbers.Count >= 5)
            {
                for (int i = 0; i < 5 && i < lastNumbers.Count; i++)
                {
                    if (weightedCandidates.ContainsKey(lastNumbers[i]))
                    {
                        // Son sayıların ağırlığını azalt, en son çıkan sayının tekrar gelme olasılığı en düşük
                        // Ancak bazen son sayıların tekrarlanma olasılığı da vardır
                        if (random.NextDouble() > 0.2) // %80 ihtimalle ceza uygula
                        {
                            int penalty = i == 0 ? 3 : (i <= 2 ? 2 : 1);
                            weightedCandidates[lastNumbers[i]] = Math.Max(1, weightedCandidates[lastNumbers[i]] - penalty);
                        }
                        else if (i > 2) // %20 ihtimalle ve son çıkanlardan değilse, tekrar gelebilir
                        {
                            weightedCandidates[lastNumbers[i]] += random.Next(1, 3);
                        }
                    }
                }
            }
            
            // Trend analizi: Son sayılarda artan veya azalan bir trend var mı?
            if (lastNumbers.Count >= 5 && random.NextDouble() < 0.6) // %60 ihtimalle trend analizi yap
            {
                bool increasingTrend = true;
                bool decreasingTrend = true;
                
                for (int i = 0; i < 4; i++)
                {
                    if (lastNumbers[i] <= lastNumbers[i + 1])
                        decreasingTrend = false;
                    if (lastNumbers[i] >= lastNumbers[i + 1])
                        increasingTrend = false;
                }
                
                if (increasingTrend && lastNumbers[0] < 30)
                {
                    // Artan trend varsa, daha büyük bir sayı tahmin et
                    // Ama rastgele bir aralık seç
                    int start = lastNumbers[0] + 1;
                    int range = Math.Min(6, 36 - start);
                    int selectedCount = Math.Min(range, 2 + random.Next(1, 4)); // 2-5 arası sayı seç
                    
                    var selectedNumbers = Enumerable.Range(start, range)
                        .OrderBy(x => random.Next())
                        .Take(selectedCount);
                        
                    foreach (var num in selectedNumbers)
                    {
                        AddOrUpdateCandidate(weightedCandidates, num, 2 + random.Next(1, 3));
                    }
                }
                else if (decreasingTrend && lastNumbers[0] > 6)
                {
                    // Azalan trend varsa, daha küçük bir sayı tahmin et
                    // Ama rastgele bir aralık seç
                    int end = lastNumbers[0] - 1;
                    int start = Math.Max(0, end - 6);
                    int range = end - start + 1;
                    int selectedCount = Math.Min(range, 2 + random.Next(1, 4)); // 2-5 arası sayı seç
                    
                    var selectedNumbers = Enumerable.Range(start, range)
                        .OrderBy(x => random.Next())
                        .Take(selectedCount);
                        
                    foreach (var num in selectedNumbers)
                    {
                        AddOrUpdateCandidate(weightedCandidates, num, 2 + random.Next(1, 3));
                    }
                }
            }
            
            // Komşu sayılar stratejisi - 9 sağ ve 9 sol komşuları dikkate al
            if (lastNumbers.Count > 0)
            {
                // Son 5 sayının 9 sağ ve 9 sol komşularını değerlendir
                for (int i = 0; i < Math.Min(5, lastNumbers.Count); i++)
                {
                    int currentNum = lastNumbers[i];
                    
                    // 9 sağ komşu
                    int rightNeighbor = (currentNum + 9) % 37;
                    // 9 sol komşu
                    int leftNeighbor = (currentNum + 28) % 37; // (currentNum - 9 + 37) % 37 ile aynı
                    
                    // Komşu sayıların frekansına göre ağırlık ver
                    int rightFreq = neighborFrequency[currentNum]["right"];
                    int leftFreq = neighborFrequency[currentNum]["left"];
                    
                    // Frekans ne kadar yüksekse, o kadar yüksek ağırlık ver
                    int rightWeight = 2 + Math.Min(5, rightFreq * 2) + (i == 0 ? 2 : 0); // Son sayıya daha fazla ağırlık
                    int leftWeight = 2 + Math.Min(5, leftFreq * 2) + (i == 0 ? 2 : 0);
                    
                    // Rastgele bir varyasyon ekle
                    rightWeight += random.Next(0, 3);
                    leftWeight += random.Next(0, 3);
                    
                    // Komşu sayıları aday listesine ekle
                    AddOrUpdateCandidate(weightedCandidates, rightNeighbor, rightWeight);
                    AddOrUpdateCandidate(weightedCandidates, leftNeighbor, leftWeight);
                    
                    // Kullanıcının bahis stratejisine uygun olarak, bu komşuların komşularına da düşük ağırlık ver
                    // Yani tahmin edilen sayının 18 sağ ve 18 sol komşusuna da bahis koyulabilir
                    int farRightNeighbor = (rightNeighbor + 9) % 37;
                    int farLeftNeighbor = (leftNeighbor + 28) % 37;
                    
                    AddOrUpdateCandidate(weightedCandidates, farRightNeighbor, rightWeight / 2);
                    AddOrUpdateCandidate(weightedCandidates, farLeftNeighbor, leftWeight / 2);
                }
                
                // Standart komşu sayılar stratejisi (1 sağ, 1 sol)
                if (random.NextDouble() < 0.2) // Daha düşük ihtimalle uygula
                {
                    int lastNum = lastNumbers[0];
                    int neighbor1 = (lastNum + 1) % 37;
                    int neighbor2 = (lastNum + 36) % 37; // -1 mod 37
                    
                    AddOrUpdateCandidate(weightedCandidates, neighbor1, 1 + random.Next(0, 2));
                    AddOrUpdateCandidate(weightedCandidates, neighbor2, 1 + random.Next(0, 2));
                }
            }
            
            // Tamamen rastgele sayılar ekle (çeşitlilik için)
            for (int i = 0; i < 3; i++)
            {
                int randomNum = random.Next(0, 37);
                AddOrUpdateCandidate(weightedCandidates, randomNum, 1 + random.Next(0, 2));
            }
            
            // Ağırlıklı seçim yap
            if (weightedCandidates.Any())
            {
                // Rastgele seçim stratejisi belirle
                double selectionStrategy = random.NextDouble();
                
                if (selectionStrategy < 0.6) // %60 ihtimalle en yüksek ağırlıklı adaylardan seç
                {
                    // En yüksek ağırlıklı adayları bul
                    int maxWeight = weightedCandidates.Values.Max();
                    double threshold = 0.5 + (random.NextDouble() * 0.3); // 0.5-0.8 arası eşik değeri
                    var topCandidates = weightedCandidates.Where(kvp => kvp.Value >= maxWeight * threshold)
                                                          .Select(kvp => kvp.Key)
                                                          .ToList();
                    
                    if (topCandidates.Count > 0)
                    {
                        return topCandidates[random.Next(topCandidates.Count)];
                    }
                }
                
                // Ağırlıklı seçim için tüm adayları listeye ekle
                foreach (var kvp in weightedCandidates)
                {
                    for (int i = 0; i < kvp.Value; i++)
                    {
                        candidates.Add(kvp.Key);
                    }
                }
                
                if (candidates.Count > 0)
                {
                    return candidates[random.Next(candidates.Count)];
                }
            }
            
            // Hiçbir strateji uygulanamadıysa, rastgele bir sayı döndür
            return random.Next(37); // 0-36 arası
        }
        
        private void AddOrUpdateCandidate(Dictionary<int, int> candidates, int number, int weight)
        {
            if (candidates.ContainsKey(number))
            {
                candidates[number] += weight;
            }
            else
            {
                candidates[number] = weight;
            }
        }

        #endregion
    }
}
