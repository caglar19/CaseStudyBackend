using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CaseStudy.Application.Models.BayTahmin
{
    public class PlayerProfile
    {
        [JsonPropertyName("player")]
        public PlayerInfo Player { get; set; }

        [JsonPropertyName("statistics")]
        public List<PlayerStatistics> Statistics { get; set; }
    }

    public class PlayerDetailedInfo : PlayerInfo
    {
        [JsonPropertyName("firstname")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastname")]
        public string LastName { get; set; }

        [JsonPropertyName("age")]
        public int Age { get; set; }

        [JsonPropertyName("birth")]
        public BirthInfo Birth { get; set; }

        [JsonPropertyName("nationality")]
        public string Nationality { get; set; }

        [JsonPropertyName("height")]
        public string Height { get; set; }

        [JsonPropertyName("weight")]
        public string Weight { get; set; }

        [JsonPropertyName("injured")]
        public bool Injured { get; set; }
    }

    public class BirthInfo
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("place")]
        public string Place { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }
    }

    public class PlayerSquad
    {
        [JsonPropertyName("team")]
        public TeamInfo Team { get; set; }

        [JsonPropertyName("players")]
        public List<SquadPlayer> Players { get; set; }
    }

    public class SquadPlayer
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("age")]
        public int Age { get; set; }

        [JsonPropertyName("number")]
        public int? Number { get; set; }

        [JsonPropertyName("position")]
        public string Position { get; set; }

        [JsonPropertyName("photo")]
        public string Photo { get; set; }
    }

    public class PlayerTeam
    {
        [JsonPropertyName("player")]
        public PlayerInfo Player { get; set; }

        [JsonPropertyName("teams")]
        public List<TeamInfo> Teams { get; set; }
    }

    public class TopPlayer
    {
        [JsonPropertyName("player")]
        public PlayerInfo Player { get; set; }

        [JsonPropertyName("statistics")]
        public List<TopPlayerStatistics> Statistics { get; set; }
    }

    public class TopPlayerStatistics
    {
        [JsonPropertyName("team")]
        public TeamInfo Team { get; set; }

        [JsonPropertyName("league")]
        public LeagueInfo League { get; set; }

        [JsonPropertyName("games")]
        public PlayerGameStats Games { get; set; }

        [JsonPropertyName("goals")]
        public PlayerGoalStats Goals { get; set; }

        [JsonPropertyName("cards")]
        public PlayerCardStats Cards { get; set; }
    }
}
