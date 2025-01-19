using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CaseStudy.Application.Models.BayTahmin
{
    public class TeamModel
    {
        [JsonPropertyName("team")]
        public TeamInfo Team { get; set; }

        [JsonPropertyName("venue")]
        public VenueInfo Venue { get; set; }
    }

    public class TeamInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("founded")]
        public int? Founded { get; set; }

        [JsonPropertyName("national")]
        public bool National { get; set; }

        [JsonPropertyName("logo")]
        public string Logo { get; set; }

        [JsonPropertyName("winner")]
        public bool? Winner { get; set; }
    }

    public class VenueInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("capacity")]
        public int? Capacity { get; set; }

        [JsonPropertyName("surface")]
        public string Surface { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }
    }

    public class TeamStatistics
    {
        [JsonPropertyName("league")]
        public LeagueInfo League { get; set; }

        [JsonPropertyName("team")]
        public TeamInfo Team { get; set; }

        [JsonPropertyName("form")]
        public string Form { get; set; }

        [JsonPropertyName("fixtures")]
        public FixturesStats Fixtures { get; set; }

        [JsonPropertyName("goals")]
        public TeamGoalsStats Goals { get; set; }

        [JsonPropertyName("biggest")]
        public BiggestStats Biggest { get; set; }

        [JsonPropertyName("clean_sheet")]
        public CleanSheetStats CleanSheet { get; set; }

        [JsonPropertyName("failed_to_score")]
        public FailedToScoreStats FailedToScore { get; set; }

        [JsonPropertyName("penalty")]
        public PenaltyStats Penalty { get; set; }

        [JsonPropertyName("lineups")]
        public List<LineupStats> Lineups { get; set; }

        [JsonPropertyName("cards")]
        public CardStats Cards { get; set; }
    }

    public class FixturesStats
    {
        [JsonPropertyName("played")]
        public StatsValue Played { get; set; }

        [JsonPropertyName("wins")]
        public StatsValue Wins { get; set; }

        [JsonPropertyName("draws")]
        public StatsValue Draws { get; set; }

        [JsonPropertyName("loses")]
        public StatsValue Loses { get; set; }
    }

    public class TeamGoalsStats
    {
        [JsonPropertyName("for")]
        public GoalsDetailStats For { get; set; }

        [JsonPropertyName("against")]
        public GoalsDetailStats Against { get; set; }
    }

    public class GoalsDetailStats
    {
        [JsonPropertyName("total")]
        public StatsValue Total { get; set; }

        [JsonPropertyName("average")]
        public StatsValue Average { get; set; }

        [JsonPropertyName("minute")]
        public Dictionary<string, MinuteStats> Minute { get; set; }
    }

    public class MinuteStats
    {
        [JsonPropertyName("total")]
        public int? Total { get; set; }

        [JsonPropertyName("percentage")]
        public string Percentage { get; set; }
    }

    public class BiggestStats
    {
        [JsonPropertyName("streak")]
        public StreakStats Streak { get; set; }

        [JsonPropertyName("wins")]
        public ScoreStats Wins { get; set; }

        [JsonPropertyName("loses")]
        public ScoreStats Loses { get; set; }

        [JsonPropertyName("goals")]
        public BiggestGoalsStats Goals { get; set; }
    }

    public class StreakStats
    {
        [JsonPropertyName("wins")]
        public int Wins { get; set; }

        [JsonPropertyName("draws")]
        public int Draws { get; set; }

        [JsonPropertyName("loses")]
        public int Loses { get; set; }
    }

    public class ScoreStats
    {
        [JsonPropertyName("home")]
        public string Home { get; set; }

        [JsonPropertyName("away")]
        public string Away { get; set; }
    }

    public class BiggestGoalsStats
    {
        [JsonPropertyName("for")]
        public ScoreStats For { get; set; }

        [JsonPropertyName("against")]
        public ScoreStats Against { get; set; }
    }

    public class CleanSheetStats
    {
        [JsonPropertyName("home")]
        public int Home { get; set; }

        [JsonPropertyName("away")]
        public int Away { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class FailedToScoreStats
    {
        [JsonPropertyName("home")]
        public int Home { get; set; }

        [JsonPropertyName("away")]
        public int Away { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class PenaltyStats
    {
        [JsonPropertyName("scored")]
        public PenaltyDetailStats Scored { get; set; }

        [JsonPropertyName("missed")]
        public PenaltyDetailStats Missed { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class PenaltyDetailStats
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("percentage")]
        public string Percentage { get; set; }
    }

    public class LineupStats
    {
        [JsonPropertyName("formation")]
        public string Formation { get; set; }

        [JsonPropertyName("played")]
        public int Played { get; set; }
    }

    public class CardStats
    {
        [JsonPropertyName("yellow")]
        public Dictionary<string, MinuteStats> Yellow { get; set; }

        [JsonPropertyName("red")]
        public Dictionary<string, MinuteStats> Red { get; set; }
    }

    public class StatsValue
    {
        [JsonPropertyName("home")]
        public int Home { get; set; }

        [JsonPropertyName("away")]
        public int Away { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
