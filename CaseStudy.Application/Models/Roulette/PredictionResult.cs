using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CaseStudy.Application.Models.Roulette
{
    /// <summary>
    /// Her tahmin ve sonucunu kaydetmek için kullanılan model
    /// </summary>
    public class PredictionResult
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        /// <summary>
        /// Tahmin edilen sayı
        /// </summary>
        public int PredictedNumber { get; set; }
        
        /// <summary>
        /// Gerçekte çıkan sayı
        /// </summary>
        public int ActualNumber { get; set; }
        
        /// <summary>
        /// Tahmin başarılı mı?
        /// </summary>
        public bool IsCorrect => PredictedNumber == ActualNumber;
        
        /// <summary>
        /// Tahmin tarihi
        /// </summary>
        public DateTime PredictionTime { get; set; }
        
        /// <summary>
        /// Her stratejinin bu tahmine katkısı (0-10 arası ağırlık)
        /// </summary>
        public Dictionary<string, int> StrategyContributions { get; set; } = new Dictionary<string, int>();
        
        /// <summary>
        /// Tahminde kullanılan tüm sayıların kopyası (tarihsel analiz için)
        /// </summary>
        public List<int> NumbersUsed { get; set; } = new List<int>();
    }
}
