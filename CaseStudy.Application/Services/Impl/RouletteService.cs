using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using CaseStudy.Application.Interfaces;
using CaseStudy.Application.Models.Roulette;
using System.Text.RegularExpressions;
using MongoDB.Driver;

// Kullanılacak modelleri açıkça belirt
using RoulettePredictionResponseModel = CaseStudy.Application.Models.Roulette.RoulettePredictionResponse;
using RouletteInitializeResponseModel = CaseStudy.Application.Models.Roulette.RouletteInitializeResponse;
using RouletteExtractNumbersResponseModel = CaseStudy.Application.Models.Roulette.RouletteExtractNumbersResponse;

namespace CaseStudy.Application.Services.Impl
{
    public class RouletteService : IRouletteService
    {
        private readonly IMongoCollection<RouletteData> _rouletteCollection;
        private readonly IMongoCollection<PredictionResult> _predictionResultsCollection;
        private readonly IMongoCollection<StrategyPerformance> _strategyPerformanceCollection;
        private readonly string _defaultRouletteId = "default";
        
        // Tahmin stratejilerinin adları
        private readonly List<string> _strategyNames = new List<string>
        {
            "hot_numbers",
            "cold_numbers",
            "odd_even_distribution",
            "high_low_distribution",
            "red_black_distribution",
            "sequence_analysis",
            "recurrence_intervals",
            "recent_numbers_penalty"
        };
        
        // Son tahmin - gerçek sonuç karşılaştırması için
        private int _lastPredictedNumber = -1;

        public RouletteService(IOptions<MongoDBSettings> mongoSettings)
        {
            try
            {
                var client = new MongoClient(mongoSettings.Value.ConnectionString);
                var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
                _rouletteCollection = database.GetCollection<RouletteData>(mongoSettings.Value.RouletteCollectionName);
                _predictionResultsCollection = database.GetCollection<PredictionResult>(mongoSettings.Value.PredictionResultsCollectionName);
                _strategyPerformanceCollection = database.GetCollection<StrategyPerformance>(mongoSettings.Value.StrategyPerformanceCollectionName);
                
                // Strateji performans kayıtlarını kontrol et ve yoksa oluştur
                InitializeStrategyPerformanceTracking().Wait();
            }
            catch
            {
                throw;
            }
        }
        
        /// <summary>
        /// Strateji performans kayıtlarını kontrol eder ve yoksa oluşturur
        /// </summary>
        private async Task InitializeStrategyPerformanceTracking()
        {
            try
            {
                foreach (var strategyName in _strategyNames)
                {
                    var strategy = await _strategyPerformanceCollection.Find(s => s.StrategyName == strategyName).FirstOrDefaultAsync();
                    
                    if (strategy == null)
                    {
                        // Yeni strateji kaydı oluştur
                        await _strategyPerformanceCollection.InsertOneAsync(new StrategyPerformance
                        {
                            StrategyName = strategyName,
                            UsageCount = 0,
                            CorrectPredictionCount = 0,
                            DynamicWeight = 50, // Başlangıçta eşit ağırlık
                            LastUpdated = DateTime.Now,
                            RecentResults = new List<bool>()
                        });
                    }
                }
            }
            catch (Exception)
            {
                // Log hata - opsiyonel
            }
        }

        private async Task<RouletteData> GetRouletteDataAsync()
        {
            return await _rouletteCollection.Find(r => r.Name == _defaultRouletteId).FirstOrDefaultAsync();
        }

        public async Task<RoulettePredictionResponseModel> InitializeNumbersAsync(List<int> initialNumbers)
        {
            try
            {
                if (initialNumbers == null || initialNumbers.Count == 0)
                {
                    return new RoulettePredictionResponseModel
                    {
                        Success = false,
                        Prediction = -1,
                        Numbers = new List<int>(),
                        ErrorMessage = "Geçerli rulet sayıları verilmedi."
                    };
                }

                var existingData = await GetRouletteDataAsync();

                if (existingData != null)
                {
                    // Mevcut veriyi güncelle
                    existingData.Numbers = initialNumbers;
                    await _rouletteCollection.ReplaceOneAsync(r => r.Name == _defaultRouletteId, existingData);
                }
                else
                {
                    // Yeni veri oluştur
                    var newData = new RouletteData
                    {
                        Name = _defaultRouletteId,
                        Numbers = initialNumbers
                    };
                    await _rouletteCollection.InsertOneAsync(newData);
                }

                // Tahmin yap
                int prediction = PredictNextNumber(initialNumbers);
                
                // Son tahmin edilen sayıyı sakla (doğruluk takibi için)
                _lastPredictedNumber = prediction;
                
                return new RoulettePredictionResponseModel
                {
                    Success = true,
                    Prediction = prediction,
                    Numbers = initialNumbers
                };
            }
            catch
            {
                return new RoulettePredictionResponseModel
                {
                    Success = false,
                    Prediction = -1,
                    Numbers = new List<int>(),
                    ErrorMessage = "Rulet sayıları yüklenirken hata oluştu."
                };
            }
        }

        public async Task<RouletteInitializeResponseModel> InitializeWithNumbers(List<int> initialNumbers)
        {
            try
            {
                if (initialNumbers == null || initialNumbers.Count == 0)
                {
                    return new RouletteInitializeResponseModel
                    {
                        Success = false,
                        NumbersCount = 0
                    };
                }

                // InitializeNumbersAsync metodunu çağırarak MongoDB'ye kaydet
                await InitializeNumbersAsync(initialNumbers);
                var data = await GetRouletteDataAsync();
                int numbersCount = data?.Numbers?.Count ?? 0;
                
                return new RouletteInitializeResponseModel
                {
                    Success = true,
                    NumbersCount = numbersCount
                };
            }
            catch
            {

                return new RouletteInitializeResponseModel
                {
                    Success = false,
                    NumbersCount = 0
                };
            }
        }

        public async Task<RoulettePredictionResponseModel> AddNumberAndPredict(int newNumber)
        {
            try
            {
                var rouletteData = await GetRouletteDataAsync();
                
                if (rouletteData == null)
                {
                    return new RoulettePredictionResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Henüz başlangıç verileri yüklenmemiş. Önce InitializeNumbersAsync metodunu çağırın.",
                        Prediction = -1
                    };
                }
                
                // Yeni sayıyı ekle
                rouletteData.Numbers.Insert(0, newNumber); // Yeni sayıyı listenin başına ekle
                
                // Veritabanını güncelle
                await _rouletteCollection.ReplaceOneAsync(r => r.Name == _defaultRouletteId, rouletteData);
                
                // Tahmin yap
                int prediction = PredictNextNumber(rouletteData.Numbers);
                
                // Son tahmin edilen sayıyı sakla (doğruluk takibi için)
                _lastPredictedNumber = prediction;
                
                return new RoulettePredictionResponseModel
                {
                    Success = true,
                    Prediction = prediction,
                    Numbers = rouletteData.Numbers
                };
            }
            catch (Exception ex)
            {
                return new RoulettePredictionResponseModel
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Prediction = -1
                };
            }
        }

        public Task<RouletteExtractNumbersResponseModel> ExtractNumbersFromHtml(string htmlContent)
        {
            try
            {

                
                if (string.IsNullOrEmpty(htmlContent))
                {
    
                    return Task.FromResult(new RouletteExtractNumbersResponseModel
                    {
                        Success = false,
                        ErrorMessage = "HTML içeriği boş veya geçersiz",
                        Numbers = new List<int>(),
                        NumbersCount = 0
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
    
                    return Task.FromResult(new RouletteExtractNumbersResponseModel
                    {
                        Success = false,
                        ErrorMessage = "HTML içeriğinde rulet sayısı bulunamadı",
                        Numbers = new List<int>(),
                        NumbersCount = 0
                    });
                }
                

                
                return Task.FromResult(new RouletteExtractNumbersResponseModel
                {
                    Success = true,
                    Numbers = numbers,
                    NumbersCount = numbers.Count
                });
            }
            catch
            {

                return Task.FromResult(new RouletteExtractNumbersResponseModel
                {
                    Success = false,
                    ErrorMessage = "HTML içeriğinden rulet sayıları çıkarılırken hata oluştu",
                    Numbers = new List<int>(),
                    NumbersCount = 0
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

            // 6. Sayıların tekrarlanma aralıklarını analiz et
            var recurrenceIntervals = AnalyzeRecurrenceIntervals(numbers);

            // Tüm stratejileri bir araya getirerek ağırlıklı bir tahmin yap
            var prediction = GeneratePrediction(hotNumbers, coldNumbers, lastNumbers, 
                oddCount, evenCount, zeroCount, lowCount, highCount, redCount, blackCount, 
                sequences, recurrenceIntervals);

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
        /// Sayıların tekrarlanma aralıklarını analiz eder
        /// </summary>
        /// <param name="numbers">Analiz edilecek sayı listesi</param>
        /// <returns>Sayıların tekrarlanma aralıklarını içeren sözlük</returns>
        private Dictionary<int, int> AnalyzeRecurrenceIntervals(List<int> numbers)
        {
            var result = new Dictionary<int, int>();
            
            if (numbers == null || numbers.Count < 10)
            {
                return result;
            }
            
            // Her sayının tekrar etme aralığını hesapla
            var lastOccurrenceIndex = new Dictionary<int, int>();
            
            for (int i = 0; i < numbers.Count; i++)
            {
                int num = numbers[i];
                
                if (lastOccurrenceIndex.ContainsKey(num))
                {
                    int interval = i - lastOccurrenceIndex[num];
                    
                    if (result.ContainsKey(num))
                    {
                        // Mevcut aralık ile ortalama al
                        result[num] = (result[num] + interval) / 2;
                    }
                    else
                    {
                        result[num] = interval;
                    }
                }
                
                // Son görülme indeksini güncelle
                lastOccurrenceIndex[num] = i;
            }
            
            return result;
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
            Dictionary<int, int> recurrenceIntervals)
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
            
            // Tekrarlanma aralıkları stratejisi - sayıların gelme sıklığını değerlendir
            if (recurrenceIntervals.Any() && lastNumbers.Count > 0)
            {
                // Tekrarlanma aralığı bilinen ve yakın zamanda gelme potansiyeli olan sayıları değerlendir
                foreach (var kvp in recurrenceIntervals.OrderBy(k => k.Value)) // Aralığı en küçük olana öncelik ver
                {
                    int number = kvp.Key;
                    int interval = kvp.Value;
                    
                    // Son çıkan sayının ne kadar önce çıktığını bul
                    int lastOccurrence = -1;
                    for (int i = 0; i < lastNumbers.Count; i++)
                    {
                        if (lastNumbers[i] == number)
                        {
                            lastOccurrence = i;
                            break;
                        }
                    }
                    
                    // Eğer sayı son çıkan sayılardan biriyse
                    if (lastOccurrence != -1)
                    {
                        // Tekrarlanma aralığına bakarak ağırlık hesapla
                        // Örneğin sayı ortalama her 5 turda bir çıkıyorsa ve 4 tur önce çıktıysa, gelme ihtimali yüksek
                        int cycleProgress = lastOccurrence + 1; // Sayının son çıkışından bu yana geçen tur sayısı
                        double completionRatio = (double)cycleProgress / interval;
                        
                        // Tamamlanma oranına göre ağırlık ver
                        // 0.8-1.2 arasındaki tamamlanma oranları en yüksek ağırlığı alır
                        int weight = 0;
                        
                        if (completionRatio >= 0.8 && completionRatio <= 1.2)
                        {
                            // Sayı yakında çıkabilir, yüksek ağırlık ver
                            weight = 6 + random.Next(0, 3);
                        }
                        else if (completionRatio > 0.5 && completionRatio < 1.5)
                        {
                            // Orta ihtimal
                            weight = 3 + random.Next(0, 3);
                        }
                        else
                        {
                            // Düşük ihtimal
                            weight = 1 + random.Next(0, 2);
                        }
                        
                        // Düşük aralıklı sayılara bonus ver (sık tekrarlanan sayılar)
                        if (interval <= 5)
                        {
                            weight += 2;
                        }
                        else if (interval <= 10)
                        {
                            weight += 1;
                        }
                        
                        AddOrUpdateCandidate(weightedCandidates, number, weight);
                    }
                }
                
                // Standart komşu sayılar stratejisi (1 sağ, 1 sol) - bunu koruyalım istatistiksel çeşitlilik için
                if (random.NextDouble() < 0.3) // %30 ihtimalle uygula
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

        #region Tahmin Doğruluk Takibi ve Strateji Analizi
        
        /// <summary>
        /// Gerçek çıkan sayıyı sisteme bildirir ve tahmin doğruluğunu günceller
        /// </summary>
        /// <param name="actualNumber">Gerçekte çıkan sayı</param>
        /// <returns>Doğruluk güncelleme sonucu</returns>
        public async Task<PredictionAccuracyResponse> RecordActualNumberAsync(int actualNumber)
        {
            try
            {
                if (_lastPredictedNumber == -1)
                {
                    return new PredictionAccuracyResponse 
                    { 
                        Success = false, 
                        ErrorMessage = "Henüz bir tahmin yapılmamış. Önce AddNumberAndPredict metodunu çağırın." 
                    };
                }
                
                // Rulet verilerini al
                var rouletteData = await GetRouletteDataAsync();
                if (rouletteData == null || rouletteData.Numbers == null || rouletteData.Numbers.Count == 0)
                {
                    return new PredictionAccuracyResponse 
                    { 
                        Success = false, 
                        ErrorMessage = "Rulet verileri bulunamadı veya boş." 
                    };
                }
                
                // Tahmin sonucunu kaydet
                var predictionResult = new PredictionResult
                {
                    PredictedNumber = _lastPredictedNumber,
                    ActualNumber = actualNumber,
                    PredictionTime = DateTime.Now,
                    NumbersUsed = new List<int>(rouletteData.Numbers),
                    StrategyContributions = new Dictionary<string, int>()
                };
                
                // Her strateji için katkıyı ekle - gerçek strateji katkıları için GeneratePrediction'ı güncellememiz gerekecek
                // Şimdilik varsayılan değerler ekliyoruz
                foreach (var strategyName in _strategyNames)
                {
                    predictionResult.StrategyContributions[strategyName] = 5; // Varsayılan orta ağırlık
                }
                
                // Sonucu MongoDB'ye kaydet
                await _predictionResultsCollection.InsertOneAsync(predictionResult);
                
                // Strateji performanslarını güncelle
                foreach (var strategyName in _strategyNames)
                {
                    var strategy = await _strategyPerformanceCollection.Find(s => s.StrategyName == strategyName).FirstOrDefaultAsync();
                    
                    if (strategy != null)
                    {
                        // Kullanım sayısını arttır ve doğruysa doğru tahmin sayısını da arttır
                        strategy.UsageCount++;
                        
                        // Tahmin edilen sayının 9 sağ ve 9 sol komşuları da doğru kabul edilecek
                        int rightNeighbor = (_lastPredictedNumber + 9) % 37; // 9 sağ komşu
                        int leftNeighbor = (_lastPredictedNumber + 28) % 37; // (currentNumber - 9 + 37) % 37 ile aynı, 9 sol komşu
                        
                        // Tam eşleşme, sağ komşu eşleşmesi veya sol komşu eşleşmesi olursa doğru say
                        bool isCorrect = _lastPredictedNumber == actualNumber || rightNeighbor == actualNumber || leftNeighbor == actualNumber;
                        
                        if (isCorrect)
                        {
                            strategy.CorrectPredictionCount++;
                        }
                        
                        // Son sonuçları güncelle
                        if (strategy.RecentResults.Count >= 100)
                        {
                            strategy.RecentResults.RemoveAt(0); // En eski sonucu çıkar
                        }
                        strategy.RecentResults.Add(isCorrect);
                        
                        // Dinamik ağırlığı son performansa göre güncelle
                        // Basit bir dinamik ayarlama: son 10 sonucun doğruluk oranına göre ağırlık ayarla
                        if (strategy.RecentResults.Count >= 10)
                        {
                            int recentCorrect = strategy.RecentResults.TakeLast(10).Count(r => r);
                            double recentAccuracy = (double)recentCorrect / 10;
                            
                            // Ağırlığı güncelle (20-80 aralığında)
                            strategy.DynamicWeight = Math.Max(20, Math.Min(80, (int)(recentAccuracy * 100)));
                        }
                        
                        strategy.LastUpdated = DateTime.Now;
                        
                        // MongoDB'yi güncelle
                        await _strategyPerformanceCollection.ReplaceOneAsync(s => s.Id == strategy.Id, strategy);
                    }
                }
                
                // Sonuçları döndür
                return await GetPredictionAccuracyAsync();
            }
            catch (Exception ex)
            {
                return new PredictionAccuracyResponse
                {
                    Success = false,
                    ErrorMessage = $"Tahmin sonucu kaydedilirken hata oluştu: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Tüm tahmin stratejilerinin performansını getirir
        /// </summary>
        /// <returns>Strateji performans sonuçları</returns>
        public async Task<List<StrategyPerformance>> GetStrategyPerformancesAsync()
        {
            try
            {
                return await _strategyPerformanceCollection.Find(_ => true).ToListAsync();
            }
            catch (Exception)
            {
                return new List<StrategyPerformance>();
            }
        }

        /// <summary>
        /// Genel tahmin doğruluk oranlarını getirir
        /// </summary>
        /// <returns>Genel doğruluk analizi</returns>
        public async Task<PredictionAccuracyResponse> GetPredictionAccuracyAsync()
        {
            try
            {
                // Tüm tahmin sonuçlarını getir
                var allPredictions = await _predictionResultsCollection.Find(_ => true).ToListAsync();
                var totalPredictions = allPredictions.Count;
                
                if (totalPredictions == 0)
                {
                    return new PredictionAccuracyResponse
                    {
                        Success = true,
                        TotalPredictions = 0,
                        CorrectPredictions = 0,
                        Last10Accuracy = 0,
                        Last50Accuracy = 0,
                        Last100Accuracy = 0,
                        MostSuccessfulStrategy = "Henüz veri yok",
                        MostSuccessfulStrategyAccuracy = 0,
                        StrategyPerformances = new List<StrategyPerformanceSummary>(),
                        Last10Results = new List<int>()
                    };
                }
                
                // Doğru tahminlerin sayısını hesapla
                var correctPredictions = allPredictions.Count(p => p.IsCorrect);
                
                // Son 10, 50 ve 100 tahminin doğruluk oranlarını hesapla
                var last10 = allPredictions.OrderByDescending(p => p.PredictionTime).Take(10).ToList();
                var last50 = allPredictions.OrderByDescending(p => p.PredictionTime).Take(50).ToList();
                var last100 = allPredictions.OrderByDescending(p => p.PredictionTime).Take(100).ToList();
                
                var last10Accuracy = last10.Count > 0 ? (double)last10.Count(p => p.IsCorrect) / last10.Count : 0;
                var last50Accuracy = last50.Count > 0 ? (double)last50.Count(p => p.IsCorrect) / last50.Count : 0;
                var last100Accuracy = last100.Count > 0 ? (double)last100.Count(p => p.IsCorrect) / last100.Count : 0;
                
                // Strateji performanslarını getir
                var strategies = await GetStrategyPerformancesAsync();
                
                // En başarılı stratejiyi bul
                var mostSuccessful = strategies.OrderByDescending(s => s.RecentAccuracyRate).FirstOrDefault();
                
                // Son 10 sonucu listele (1: doğru, 0: yanlış)
                var last10Results = last10.Select(p => p.IsCorrect ? 1 : 0).ToList();
                
                // Strateji performans özetlerini oluştur
                var strategyPerformances = strategies.Select(s => new StrategyPerformanceSummary
                {
                    StrategyName = s.StrategyName,
                    AccuracyRate = s.AccuracyRate,
                    CurrentWeight = s.DynamicWeight
                }).ToList();
                
                return new PredictionAccuracyResponse
                {
                    Success = true,
                    TotalPredictions = totalPredictions,
                    CorrectPredictions = correctPredictions,
                    Last10Accuracy = last10Accuracy,
                    Last50Accuracy = last50Accuracy,
                    Last100Accuracy = last100Accuracy,
                    MostSuccessfulStrategy = mostSuccessful?.StrategyName ?? "Veri yetersiz",
                    MostSuccessfulStrategyAccuracy = mostSuccessful?.AccuracyRate ?? 0,
                    StrategyPerformances = strategyPerformances,
                    Last10Results = last10Results
                };
            }
            catch (Exception ex)
            {
                return new PredictionAccuracyResponse
                {
                    Success = false,
                    ErrorMessage = $"Tahmin doğruluk analizi yapılırken hata oluştu: {ex.Message}"
                };
            }
        }
        
        #endregion
    }
}
