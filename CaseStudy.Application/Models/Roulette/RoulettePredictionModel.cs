using System.Collections.Generic;

namespace CaseStudy.Application.Models.Roulette
{
    /// <summary>
    /// İlk rulet sayılarını yüklemek için kullanılan model
    /// </summary>
    public class RouletteInitializeRequest
    {
        /// <summary>
        /// İlk yüklenecek rulet sayıları
        /// </summary>
        public List<int> InitialNumbers { get; set; } = new List<int>();
    }

    /// <summary>
    /// İlk yükleme sonucu
    /// </summary>
    public class RouletteInitializeResponse
    {
        /// <summary>
        /// İşlem başarılı mı
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Yüklenen sayı adedi
        /// </summary>
        public int NumbersCount { get; set; }
    }

    /// <summary>
    /// Yeni rulet sayısı eklemek için kullanılan model
    /// </summary>
    public class RouletteAddNumberRequest
    {
        /// <summary>
        /// Yeni gelen rulet sayısı
        /// </summary>
        public int NewNumber { get; set; }
    }

    /// <summary>
    /// Rulet tahmini sonucu
    /// </summary>
    public class RoulettePredictionResponse
    {
        /// <summary>
        /// İşlem başarılı mı?
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Tahmin edilen bir sonraki sayı
        /// </summary>
        public int Prediction { get; set; }
        
        /// <summary>
        /// Mevcut rulet sayıları
        /// </summary>
        public List<int> Numbers { get; set; } = new List<int>();
        
        /// <summary>
        /// Varsa hata mesajı
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Tahmin için kullanılan stratejiler
        /// </summary>
        public List<string> Strategies { get; set; } = new List<string>
        {
            "Sıcak Sayılar: En sık tekrar eden sayılar",
            "Soğuk Sayılar: Uzun süredir çıkmayan sayılar",
            "Dizi Analizi: Tekrar eden sayı dizileri",
            "Dağılım Analizi: Çift/tek, kırmızı/siyah, yüksek/düşük",
            "Tekrarlanma Aralığı Analizi: Sayıların ne sıklıkla geldiği"
        };
    }
    
    /// <summary>
    /// HTML içeriğinden rulet sayılarını çıkarmak için kullanılan model
    /// </summary>
    public class RouletteExtractNumbersRequest
    {
        /// <summary>
        /// Rulet sayılarını içeren HTML içeriği
        /// </summary>
        public string? HtmlContent { get; set; }
    }
    
    /// <summary>
    /// HTML içeriğinden çıkarılan rulet sayıları sonucu
    /// </summary>
    public class RouletteExtractNumbersResponse
    {
        /// <summary>
        /// İşlem başarılı mı
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Çıkarılan rulet sayıları
        /// </summary>
        public List<int> Numbers { get; set; } = new List<int>();
        
        /// <summary>
        /// Çıkarılan sayı adedi
        /// </summary>
        public int NumbersCount { get; set; }
        
        /// <summary>
        /// Hata mesajı (başarısız olursa)
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
