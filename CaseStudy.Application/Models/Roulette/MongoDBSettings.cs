namespace CaseStudy.Application.Models.Roulette
{
    public class MongoDBSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string RouletteCollectionName { get; set; } = string.Empty;
        public string PredictionResultsCollectionName { get; set; } = string.Empty;
        public string StrategyPerformanceCollectionName { get; set; } = string.Empty;
        public string PredictionRecordsCollectionName { get; set; } = string.Empty;
    }
}
