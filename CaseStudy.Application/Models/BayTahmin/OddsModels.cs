using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CaseStudy.Application.Models.BayTahmin
{
    public class OddsModel
    {
        [JsonPropertyName("league")]
        public LeagueInfo League { get; set; }

        [JsonPropertyName("fixture")]
        public FixtureInfo Fixture { get; set; }

        [JsonPropertyName("update")]
        public DateTime Update { get; set; }

        [JsonPropertyName("bookmakers")]
        public List<BookmakerOdds> Bookmakers { get; set; }
    }

    public class BookmakerOdds
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("bets")]
        public List<BetOdds> Bets { get; set; }
    }

    public class BetOdds
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("values")]
        public List<OddsValue> Values { get; set; }
    }

    public class OddsValue
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("odd")]
        public string Odd { get; set; }
    }

    public class OddsMapping
    {
        [JsonPropertyName("fixture")]
        public FixtureInfo Fixture { get; set; }

        [JsonPropertyName("bookmaker")]
        public BookmakerInfo Bookmaker { get; set; }

        [JsonPropertyName("bet")]
        public BetInfo Bet { get; set; }
    }

    public class BookmakerInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class BetInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
