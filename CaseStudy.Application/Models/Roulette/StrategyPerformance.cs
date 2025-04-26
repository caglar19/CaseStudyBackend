using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CaseStudy.Application.Models.Roulette
{
    /// <summary>
    /// Tahmin stratejilerinin performansını izlemek için kullanılan model
    /// </summary>
    public class StrategyPerformance
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        /// <summary>
        /// Strateji adı (hot_numbers, cold_numbers, recurrence_intervals, vb.)
        /// </summary>
        public string StrategyName { get; set; } = string.Empty;
        
        /// <summary>
        /// Bu strateji kaç kez kullanıldı
        /// </summary>
        public int UsageCount { get; set; }
        
        /// <summary>
        /// Bu strateji kaç kez doğru tahmin üretti
        /// </summary>
        public int CorrectPredictionCount { get; set; }
        
        /// <summary>
        /// Stratejinin dinamik ağırlığı (0-100 arası, performansa göre ayarlanır)
        /// </summary>
        public int DynamicWeight { get; set; } = 50; // Başlangıçta orta ağırlık
        
        /// <summary>
        /// Son güncellenme tarihi
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// Doğruluk oranını hesaplar
        /// </summary>
        [BsonIgnore]
        public double AccuracyRate => UsageCount > 0 ? (double)CorrectPredictionCount / UsageCount : 0;
        
        /// <summary>
        /// Son 100 tahminin sonuçları (doğru/yanlış)
        /// </summary>
        public List<bool> RecentResults { get; set; } = new List<bool>();
        
        /// <summary>
        /// Son 100 tahminin doğruluk oranı
        /// </summary>
        [BsonIgnore]
        public double RecentAccuracyRate
        {
            get
            {
                if (RecentResults.Count == 0) return 0;
                int correctCount = 0;
                foreach (var result in RecentResults)
                {
                    if (result) correctCount++;
                }
                return (double)correctCount / RecentResults.Count;
            }
        }
    }
}
