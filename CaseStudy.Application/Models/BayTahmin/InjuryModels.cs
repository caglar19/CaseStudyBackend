using System;
using System.Text.Json.Serialization;

namespace CaseStudy.Application.Models.BayTahmin
{
    public class InjuryModel
    {
        [JsonPropertyName("player")]
        public InjuryPlayerInfo Player { get; set; }

        [JsonPropertyName("team")]
        public TeamInfo Team { get; set; }

        [JsonPropertyName("fixture")]
        public FixtureInfo Fixture { get; set; }

        [JsonPropertyName("league")]
        public LeagueInfo League { get; set; }
    }

    public class InjuryPlayerInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("photo")]
        public string Photo { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
    }
}
