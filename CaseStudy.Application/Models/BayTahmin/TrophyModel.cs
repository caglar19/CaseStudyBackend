using System.Text.Json.Serialization;

namespace CaseStudy.Application.Models.BayTahmin
{
    public class TrophyModel
    {
        [JsonPropertyName("player")]
        public PlayerInfo Player { get; set; }

        [JsonPropertyName("season")]
        public string Season { get; set; }

        [JsonPropertyName("place")]
        public string Place { get; set; }

        [JsonPropertyName("league")]
        public string League { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }
    }
}
