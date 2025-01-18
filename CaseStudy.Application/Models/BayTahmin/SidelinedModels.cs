using System;
using System.Text.Json.Serialization;

namespace CaseStudy.Application.Models.BayTahmin
{
    public class SidelinedModel
    {
        [JsonPropertyName("player")]
        public PlayerInfo Player { get; set; }

        [JsonPropertyName("team")]
        public TeamInfo Team { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("start")]
        public DateTime Start { get; set; }

        [JsonPropertyName("end")]
        public DateTime? End { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }
    }
}
