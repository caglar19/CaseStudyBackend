using System;
using System.Collections.Generic;

namespace CaseStudy.Application.Models.Roulette
{
    /// <summary>
    /// Tahmin doğruluk analiz sonuçlarını taşıyan yanıt modeli
    /// </summary>
    public class PredictionAccuracyResponse
    {
        /// <summary>
        /// İşlem başarılı mı?
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Varsa hata mesajı
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Toplam yapılan tahmin sayısı
        /// </summary>
        public int TotalPredictions { get; set; }
        
        /// <summary>
        /// Doğru tahmin sayısı
        /// </summary>
        public int CorrectPredictions { get; set; }
        
        /// <summary>
        /// Genel doğruluk oranı (0-1 arası)
        /// </summary>
        public double OverallAccuracy => TotalPredictions > 0 ? (double)CorrectPredictions / TotalPredictions : 0;
        
        /// <summary>
        /// Son 10 tahminin doğruluk oranı
        /// </summary>
        public double Last10Accuracy { get; set; }
        
        /// <summary>
        /// Son 50 tahminin doğruluk oranı
        /// </summary>
        public double Last50Accuracy { get; set; }
        
        /// <summary>
        /// Son 100 tahminin doğruluk oranı
        /// </summary>
        public double Last100Accuracy { get; set; }
        
        /// <summary>
        /// En başarılı strateji
        /// </summary>
        public string MostSuccessfulStrategy { get; set; } = string.Empty;
        
        /// <summary>
        /// En başarılı strateji doğruluk oranı
        /// </summary>
        public double MostSuccessfulStrategyAccuracy { get; set; }
        
        /// <summary>
        /// Her stratejinin detaylı performansı
        /// </summary>
        public List<StrategyPerformanceSummary> StrategyPerformances { get; set; } = new List<StrategyPerformanceSummary>();
        
        /// <summary>
        /// Son 10 tahmin sonucu (1: doğru, 0: yanlış)
        /// </summary>
        public List<int> Last10Results { get; set; } = new List<int>();
    }
    
    /// <summary>
    /// Strateji performans özeti
    /// </summary>
    public class StrategyPerformanceSummary
    {
        /// <summary>
        /// Strateji adı
        /// </summary>
        public string StrategyName { get; set; } = string.Empty;
        
        /// <summary>
        /// Doğruluk oranı
        /// </summary>
        public double AccuracyRate { get; set; }
        
        /// <summary>
        /// Mevcut dinamik ağırlık
        /// </summary>
        public int CurrentWeight { get; set; }
    }
}
