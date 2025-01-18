using System.Text.Json.Serialization;

namespace CaseStudy.Application.Models.BayTahmin
{
    public class TrophyModel
    {
        [JsonPropertyName("player")]
        public PlayerInfo Player { get; set; }

        [JsonPropertyName("trophies")]
        public TrophyInfo[] Trophies { get; set; }
    }

    public class TrophyInfo
    {
        [JsonPropertyName("league")]
        public string League { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("season")]
        public string Season { get; set; }

        [JsonPropertyName("place")]
        public string Place { get; set; }
    }
}
