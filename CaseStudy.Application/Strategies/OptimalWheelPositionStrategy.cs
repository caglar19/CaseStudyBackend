using System;
using System.Collections.Generic;
using System.Linq;

namespace CaseStudy.Application.Strategies
{
    /// <summary>
    /// En iyi 3 stratejinin tahminlerini rulet çarkının fiziksel dizilimine göre değerlendirerek
    /// en optimum tahmin yapan strateji
    /// </summary>
    public class OptimalWheelPositionStrategy : IPredictionStrategy
    {
        // Rulet çarkındaki sayıların fiziksel dizilimi (saat yönünde)
        private readonly int[] _wheelSequence = new int[]
        {
            0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10, 5, 
            24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
        };

        /// <summary>
        /// Stratejinin adı
        /// </summary>
        public string Name => "Optimal Çark Pozisyon Stratejisi";

        /// <summary>
        /// En iyi 3 stratejinin tahmini kullanarak daha akıllı bir tahmin yapan algoritma.
        /// Bu strateji kendi tahminini yapmak yerine son 3 tahmini alıp değerlendirecek.
        /// </summary>
        /// <param name="numbers">Geçmiş rulet sayıları</param>
        /// <returns>Tahmin edilen sayı</returns>
        public int PredictNextNumber(List<int> numbers)
        {
            try
            {
                if (numbers == null || numbers.Count < 10)
                {
                    return new Random().Next(0, 37); // Yeterli veri yoksa rastgele sayı döndür
                }

                // Son 3 sayının çıkma olasılığını değerlendirerek tahminde bulun
                var lastThreeNumbers = numbers.Take(3).ToList();
                
                // Tahminlerin çark üzerindeki pozisyonlarını bul ve başarı oranlarını ata
                var predictions = new List<(int position, double weight)>();
                
                foreach (var number in lastThreeNumbers)
                {
                    int position = Array.IndexOf(_wheelSequence, number);
                    if (position >= 0)
                    {
                        // Son çıkan sayıya daha yüksek ağırlık ver
                        double weight = 100.0 - (lastThreeNumbers.IndexOf(number) * 20); // İlk sayı %100, ikinci %80, üçüncü %60
                        predictions.Add((position, weight));
                    }
                }

                // En sık kullanılan sayıları da ekle (son 30 sayıdan)
                var recentNumbers = numbers.Take(30).ToList();
                var frequentNumbers = recentNumbers
                    .GroupBy(n => n)
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .Select(g => g.Key);

                foreach (var number in frequentNumbers)
                {
                    if (!lastThreeNumbers.Contains(number)) // Zaten eklenmemişse
                    {
                        int position = Array.IndexOf(_wheelSequence, number);
                        if (position >= 0)
                        {
                            // Sık kullanılan sayılara orta düzey ağırlık ver
                            predictions.Add((position, 50.0));
                        }
                    }
                }
                
                // Eğer geçerli tahmin yoksa rastgele sayı döndür
                if (predictions.Count == 0)
                {
                    return new Random().Next(0, 37);
                }
                
                // Çark üzerindeki optimal konumu ağırlıklı olarak hesapla
                int optimalPosition = FindOptimalWheelPositionWithWeighting(predictions);
                
                // Optimal konumdaki sayıyı döndür
                return _wheelSequence[optimalPosition];
            }
            catch (Exception)
            {
                // Hata durumunda rastgele sayı döndür
                return new Random().Next(0, 37);
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
            // Tam isabet kontrolü
            if (predictedNumber == actualNumber)
            {
                return true;
            }
            
            // Komşu sayılar kontrolü
            if (neighbors != null && neighbors.Contains(actualNumber))
            {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// En iyi stratejilerin tahminlerini başarı oranlarına göre ağırlıklandırarak optimal konumu bulur
        /// </summary>
        /// <param name="predictions">Stratejilerin çark üzerindeki pozisyonları ve ağırlıkları</param>
        /// <returns>Optimal pozisyon</returns>
        private int FindOptimalWheelPositionWithWeighting(List<(int position, double weight)> predictions)
        {
            if (predictions.Count == 0)
                return 0; // Varsayılan olarak 0 döndür
            
            // Çark üzerinde her pozisyon için ağırlıklı puan hesapla
            int wheelSize = _wheelSequence.Length;
            double[] scores = new double[wheelSize];
            
            for (int pos = 0; pos < wheelSize; pos++)
            {
                double score = 0;
                
                foreach (var (predPos, weight) in predictions)
                {
                    // İki yöndeki mesafeyi hesapla ve kısa olanı kullan
                    int clockwiseDistance = (predPos - pos + wheelSize) % wheelSize;
                    int counterClockwiseDistance = (pos - predPos + wheelSize) % wheelSize;
                    int distance = Math.Min(clockwiseDistance, counterClockwiseDistance);
                    
                    // Mesafe azaldıkça ve ağırlık arttıkça skor artar
                    // Mesafe 0 ise tam ağırlığı kullan, mesafe arttıkça etkiyi azalt
                    double distanceEffect = 1.0 / (1 + distance * 0.5); // Mesafe arttıkça etkisi azalır
                    
                    // Başarı oranı (weight) yüksek olan strateji tahminlerini daha fazla önemse
                    score += weight * distanceEffect;
                }
                
                scores[pos] = score;
            }
            
            // En yüksek skora sahip pozisyonu bul
            int bestPosition = 0;
            double bestScore = scores[0];
            
            for (int i = 1; i < wheelSize; i++)
            {
                if (scores[i] > bestScore)
                {
                    bestScore = scores[i];
                    bestPosition = i;
                }
            }
            
            return bestPosition;
        }
    }
}
