using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CaseStudy.Application.Models.BayTahmin
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("get")]
        public string Get { get; set; }

        [JsonPropertyName("parameters")]
        public Dictionary<string, string> Parameters { get; set; }

        [JsonPropertyName("errors")]
        public List<ApiError> Errors { get; set; }

        [JsonPropertyName("results")]
        public int Results { get; set; }

        [JsonPropertyName("paging")]
        public ApiPaging Paging { get; set; }

        [JsonPropertyName("response")]
        public List<T> Response { get; set; }
    }

    public class ApiError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class ApiPaging
    {
        [JsonPropertyName("current")]
        public int Current { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class CountryModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("flag")]
        public string Flag { get; set; }
    }

    public class LeagueModel
    {
        [JsonPropertyName("league")]
        public LeagueInfo League { get; set; }

        [JsonPropertyName("country")]
        public CountryModel Country { get; set; }

        [JsonPropertyName("seasons")]
        public List<SeasonInfo> Seasons { get; set; }
    }

    public class LeagueInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("logo")]
        public string Logo { get; set; }
    }

    public class SeasonInfo
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("start")]
        public string Start { get; set; }

        [JsonPropertyName("end")]
        public string End { get; set; }

        [JsonPropertyName("current")]
        public bool Current { get; set; }

        [JsonPropertyName("coverage")]
        public SeasonCoverage Coverage { get; set; }
    }

    public class SeasonCoverage
    {
        [JsonPropertyName("fixtures")]
        public FixtureCoverage Fixtures { get; set; }

        [JsonPropertyName("standings")]
        public bool Standings { get; set; }

        [JsonPropertyName("players")]
        public bool Players { get; set; }

        [JsonPropertyName("top_scorers")]
        public bool TopScorers { get; set; }

        [JsonPropertyName("predictions")]
        public bool Predictions { get; set; }

        [JsonPropertyName("odds")]
        public bool Odds { get; set; }
    }

    public class FixtureCoverage
    {
        [JsonPropertyName("events")]
        public bool Events { get; set; }

        [JsonPropertyName("lineups")]
        public bool Lineups { get; set; }

        [JsonPropertyName("statistics_fixtures")]
        public bool StatisticsFixtures { get; set; }

        [JsonPropertyName("statistics_players")]
        public bool StatisticsPlayers { get; set; }
    }
}
