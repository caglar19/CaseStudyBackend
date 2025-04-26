using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CaseStudy.Application.Models.Roulette
{
    /// <summary>
    /// Tahmin kayıtlarını tutan model
    /// </summary>
    public class PredictionRecord
    {
        /// <summary>
        /// MongoDB document ID
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        /// <summary>
        /// Tahmin yapılan tarih
        /// </summary>
        public DateTime PredictionDate { get; set; }
        
        /// <summary>
        /// Tahmin edilen sayı
        /// </summary>
        public int PredictedNumber { get; set; }
        
        /// <summary>
        /// Gerçek çıkan sayı (null ise henüz bilinmiyor)
        /// </summary>
        public int? ActualNumber { get; set; }
        
        /// <summary>
        /// Tahmin doğru mu? (9-sağ/9-sol komşu kuralı uygulanır)
        /// </summary>
        public bool? IsCorrect { get; set; }
        
        /// <summary>
        /// Tahminden önceki son 5 sayı listesi (tahmin bağlamı)
        /// </summary>
        public int[] Context { get; set; } = Array.Empty<int>();
        
        /// <summary>
        /// Tahmin için kullanılan strateji
        /// </summary>
        public string Strategy { get; set; } = "MultiTimeScaleAnalysis";
        
        /// <summary>
        /// Tahminin geçerli komşuları (9-sağ/9-sol)
        /// </summary>
        public int[] Neighbors { get; set; } = Array.Empty<int>();
    }
}
