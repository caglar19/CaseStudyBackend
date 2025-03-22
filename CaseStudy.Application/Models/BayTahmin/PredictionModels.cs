using System.Text.Json.Serialization;

namespace CaseStudy.Application.Models.BayTahmin
{
    public class Prediction
    {
        [JsonPropertyName("predictions")]
        public PredictionInfo Predictions { get; set; }

        [JsonPropertyName("comparison")]
        public ComparisonInfo Comparison { get; set; }
    }

    public class PredictionInfo
    {
        [JsonPropertyName("winner")]
        public WinnerInfo Winner { get; set; }

        [JsonPropertyName("win_or_draw")]
        public bool? WinOrDraw { get; set; }

        [JsonPropertyName("under_over")]
        public string UnderOver { get; set; }

        [JsonPropertyName("goals")]
        public ComparisonValue Goals { get; set; }

        [JsonPropertyName("advice")]
        public string Advice { get; set; }

        [JsonPropertyName("percent")]
        public PercentInfo Percent { get; set; }
    }

    public class WinnerInfo
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }
    }

    public class PercentInfo
    {
        [JsonPropertyName("home")]
        public string Home { get; set; }

        [JsonPropertyName("draw")]
        public string Draw { get; set; }

        [JsonPropertyName("away")]
        public string Away { get; set; }
    }

    public class ComparisonInfo
    {
        [JsonPropertyName("form")]
        public ComparisonValue Form { get; set; }

        [JsonPropertyName("att")]
        public ComparisonValue Att { get; set; }

        [JsonPropertyName("def")]
        public ComparisonValue Def { get; set; }

        [JsonPropertyName("poisson_distribution")]
        public ComparisonValue PoissonDistribution { get; set; }

        [JsonPropertyName("h2h")]
        public ComparisonValue H2h { get; set; }

        [JsonPropertyName("goals")]
        public ComparisonValue Goals { get; set; }

        [JsonPropertyName("total")]
        public ComparisonValue Total { get; set; }
    }

    public class ComparisonValue
    {
        [JsonPropertyName("home")]
        public string Home { get; set; }

        [JsonPropertyName("away")]
        public string Away { get; set; }
    }
}
