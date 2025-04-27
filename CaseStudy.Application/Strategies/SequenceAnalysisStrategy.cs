using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    /// <summary>
    /// Dizi Analizi Stratejisi - Sayı dizilerindeki örüntüleri analiz ederek tahmin yapar
    /// </summary>
    public class SequenceAnalysisStrategy : IPredictionStrategy
    {
        /// <summary>
        /// Stratejinin adı
        /// </summary>
        public string Name => "sequence_analysis";

        /// <summary>
        /// Bir sonraki sayıyı tahmin eder
        /// </summary>
        /// <param name="numbers">Tüm rulet sayıları listesi (başta en son eklenen)</param>
        /// <returns>Tahmin edilen sayı</returns>
        public int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count < 5) // Dizi analizi için en az 5 sayı gerekir
            {
                return new Random(DateTime.Now.Millisecond).Next(0, 37);
            }

            var random = new Random(DateTime.Now.Millisecond);
            
            // Sayı dizilerini analiz et
            var sequences = AnalyzeSequences(numbers);
            var lastNumbers = numbers.Take(3).ToList(); // Son 3 sayı
            
            // Olası adayların ağırlıklarını tutacak sözlük
            var candidateWeights = new Dictionary<int, int>();
            
            // Tüm sayılara başlangıç değeri atama
            for (int i = 0; i <= 36; i++)
            {
                candidateWeights[i] = 1;
            }
            
            // Dizi analizi - tam ve kısmi eşleşmeler
            if (sequences.Any() && lastNumbers.Count >= 2)
            {
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
                        AddOrUpdateCandidate(candidateWeights, seq[2], 6 + random.Next(1, 4));
                    }
                    
                    // Kısmi eşleşme kontrolü - son sayı bir dizinin parçası mı?
                    if (seq.Count >= 3 && lastNumbers.Count >= 1)
                    {
                        for (int i = 0; i < seq.Count - 1; i++)
                        {
                            if (lastNumbers[0] == seq[i])
                            {
                                // Kısmi eşleşme bulundu, bu durumda seq[i+1] tahmin edilir
                                AddOrUpdateCandidate(candidateWeights, seq[i+1], 3 + random.Next(0, 3));
                                break;
                            }
                        }
                    }
                }
            }
            
            // Artan/azalan trend analizi
            if (lastNumbers.Count >= 3)
            {
                bool increasingTrend = true;
                bool decreasingTrend = true;
                
                for (int i = 0; i < lastNumbers.Count - 1; i++)
                {
                    if (lastNumbers[i] <= lastNumbers[i + 1])
                        decreasingTrend = false;
                    if (lastNumbers[i] >= lastNumbers[i + 1])
                        increasingTrend = false;
                }
                
                if (increasingTrend && lastNumbers[0] < 30)
                {
                    // Artan trend varsa, daha büyük bir sayı tahmin et
                    int start = lastNumbers[0] + 1;
                    int range = Math.Min(6, 36 - start);
                    
                    for (int offset = 1; offset <= range; offset++)
                    {
                        AddOrUpdateCandidate(candidateWeights, start + offset - 1, 3 - Math.Min(2, offset));
                    }
                }
                else if (decreasingTrend && lastNumbers[0] > 6)
                {
                    // Azalan trend varsa, daha küçük bir sayı tahmin et
                    int end = lastNumbers[0] - 1;
                    int start = Math.Max(0, end - 6);
                    int range = end - start + 1;
                    
                    for (int offset = 0; offset < range; offset++)
                    {
                        AddOrUpdateCandidate(candidateWeights, end - offset, 3 - Math.Min(2, offset));
                    }
                }
            }
            
            // Son 3 sayıyı aday listesinden zayıflat (cezalandır)
            foreach (var num in lastNumbers.Take(3))
            {
                if (candidateWeights.ContainsKey(num))
                {
                    candidateWeights[num] = Math.Max(1, candidateWeights[num] - 2);
                }
            }
            
            // En yüksek ağırlığa sahip adayları bul
            int maxWeight = candidateWeights.Values.Max();
            var topCandidates = candidateWeights
                .Where(kvp => kvp.Value >= maxWeight * 0.8)
                .Select(kvp => kvp.Key)
                .ToList();
            
            // Eğer belirgin bir aday yoksa, rastgele bir sayı döndür
            if (topCandidates.Count == 0)
            {
                return random.Next(0, 37);
            }
            
            // En yüksek ağırlıklı adaylardan rastgele birini seç
            return topCandidates[random.Next(topCandidates.Count)];
        }

        /// <summary>
        /// Sayı dizilerini analiz ederek tekrarlanan desenleri bulur
        /// </summary>
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
        /// Aday sayıların ağırlığını artırır veya ekler
        /// </summary>
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

        /// <summary>
        /// Tahminin gerçek sonuçla doğruluğunu kontrol eder
        /// </summary>
        /// <param name="predictedNumber">Tahmin edilen sayı</param>
        /// <param name="actualNumber">Gerçek sayı</param>
        /// <param name="neighbors">Tahmin edilen sayının komşuları</param>
        /// <returns>Tahmin doğru ise true, değilse false</returns>
        public bool CheckPredictionAccuracy(int predictedNumber, int actualNumber, int[] neighbors)
        {
            // Tahmin doğrudan doğru mu?
            if (predictedNumber == actualNumber)
            {
                return true;
            }
            
            // Tahmin edilen sayının komşuları içinde mi?
            if (neighbors != null && neighbors.Contains(actualNumber))
            {
                return true;
            }
            
            return false;
        }
    }
}
