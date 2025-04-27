using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    /// <summary>
    /// Tekrarlanma Aralıkları Stratejisi - Sayıların tekrarlanma sıklığını ve döngülerini analiz ederek tahmin yapar
    /// </summary>
    public class RecurrenceIntervalsStrategy : IPredictionStrategy
    {
        /// <summary>
        /// Stratejinin adı
        /// </summary>
        public string Name => "recurrence_intervals";

        /// <summary>
        /// Bir sonraki sayıyı tahmin eder
        /// </summary>
        /// <param name="numbers">Tüm rulet sayıları listesi (başta en son eklenen)</param>
        /// <returns>Tahmin edilen sayı</returns>
        public int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count < 10)
            {
                return new Random(DateTime.Now.Millisecond).Next(0, 37);
            }

            var random = new Random(DateTime.Now.Millisecond);
            
            // Tekrarlanma aralıkları stratejisi - sayıların gelme sıklığını değerlendir
            var recurrenceIntervals = AnalyzeRecurrenceIntervals(numbers);
            var lastNumbers = numbers.Take(Math.Min(25, numbers.Count)).ToList();
            
            // Tahmini sayılar ve ağırlıkları
            var candidateWeights = new Dictionary<int, int>();
            
            // Tüm olası sayılar için başlangıç değeri
            for (int i = 0; i <= 36; i++)
            {
                candidateWeights[i] = 1;
            }
            
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
                    
                    // Ağırlığı güncelle
                    if (candidateWeights.ContainsKey(number))
                    {
                        candidateWeights[number] += weight;
                    }
                    else
                    {
                        candidateWeights[number] = weight;
                    }
                }
            }
            
            // Son 3 sayının ağırlığını azalt (cezalandır)
            var last3 = numbers.Take(3).ToHashSet();
            foreach (var num in last3)
            {
                if (candidateWeights.ContainsKey(num))
                {
                    candidateWeights[num] = Math.Max(1, candidateWeights[num] - 2);
                }
            }
            
            // En yüksek ağırlıklı sayıları bul
            int maxWeight = candidateWeights.Values.Max();
            var topCandidates = candidateWeights
                .Where(kvp => kvp.Value >= maxWeight * 0.75) // En az max ağırlığın %75'i kadar olan adaylar
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
