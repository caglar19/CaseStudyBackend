using System.Collections.Generic;

namespace CaseStudy.Application.Models.Roulette
{
    /// <summary>
    /// Rulet tahmin isteği modeli
    /// </summary>
    public class RoulettePredictionRequest
    {
        /// <summary>
        /// İlk yüklemede kullanılacak rulet sayıları (ilk kez gönderildiğinde)
        /// </summary>
        public List<int>? InitialNumbers { get; set; }

        /// <summary>
        /// Yeni gelen rulet sayısı (sonraki isteklerde)
        /// </summary>
        public int? NewNumber { get; set; }
    }

    /// <summary>
    /// Rulet tahmini sonucu
    /// </summary>
    public class RoulettePredictionResponse
    {
        /// <summary>
        /// Tahmin edilen bir sonraki sayı
        /// </summary>
        public int PredictedNumber { get; set; }
        
        /// <summary>
        /// Mevcut rulet sayıları
        /// </summary>
        public List<int> Numbers { get; set; } = new List<int>();
        
        /// <summary>
        /// Tahmin için kullanılan stratejiler
        /// </summary>
        public List<string> Strategies { get; set; } = new List<string>
        {
            "Sıcak Sayılar: En sık tekrar eden sayılar",
            "Soğuk Sayılar: Uzun süredir çıkmayan sayılar",
            "Dizi Analizi: Tekrar eden sayı dizileri",
            "Dağılım Analizi: Çift/tek, kırmızı/siyah, yüksek/düşük"
        };
    }
}
