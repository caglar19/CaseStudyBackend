using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CaseStudy.Application.Models.BayTahmin
{
    public class FixtureRound
    {
        [JsonPropertyName("response")]
        public List<string> Rounds { get; set; }
    }

    public class Fixture
    {
        [JsonPropertyName("fixture")]
        public FixtureInfo Info { get; set; }

        [JsonPropertyName("league")]
        public FixtureLeagueInfo League { get; set; }

        [JsonPropertyName("teams")]
        public TeamsInfo Teams { get; set; }

        [JsonPropertyName("goals")]
        public GoalsInfo Goals { get; set; }

        [JsonPropertyName("score")]
        public ScoreInfo Score { get; set; }
    }

    public class FixtureInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("referee")]
        public string Referee { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("venue")]
        public VenueInfo Venue { get; set; }

        [JsonPropertyName("status")]
        public FixtureStatus Status { get; set; }

        [JsonPropertyName("league")]
        public FixtureLeagueStandingInfo LeagueStanding { get; set; }
    }

    public class FixtureStatus
    {
        [JsonPropertyName("long")]
        public string Long { get; set; }

        [JsonPropertyName("short")]
        public string Short { get; set; }

        [JsonPropertyName("elapsed")]
        public int? Elapsed { get; set; }
    }

    public class GoalsInfo
    {
        [JsonPropertyName("home")]
        public int? Home { get; set; }

        [JsonPropertyName("away")]
        public int? Away { get; set; }
    }

    public class ScoreInfo
    {
        [JsonPropertyName("halftime")]
        public GoalsInfo Halftime { get; set; }

        [JsonPropertyName("fulltime")]
        public GoalsInfo Fulltime { get; set; }

        [JsonPropertyName("extratime")]
        public GoalsInfo Extratime { get; set; }

        [JsonPropertyName("penalty")]
        public GoalsInfo Penalty { get; set; }
    }

    public class FixtureLeagueStandingInfo
    {
        [JsonPropertyName("home_position")]
        public int? HomePosition { get; set; }

        [JsonPropertyName("away_position")]
        public int? AwayPosition { get; set; }

        [JsonPropertyName("home_points")]
        public int? HomePoints { get; set; }

        [JsonPropertyName("away_points")]
        public int? AwayPoints { get; set; }

        [JsonPropertyName("home_goals_for")]
        public int? HomeGoalsFor { get; set; }

        [JsonPropertyName("away_goals_for")]
        public int? AwayGoalsFor { get; set; }

        [JsonPropertyName("home_goals_against")]
        public int? HomeGoalsAgainst { get; set; }

        [JsonPropertyName("away_goals_against")]
        public int? AwayGoalsAgainst { get; set; }
    }

    public class FixtureLeagueInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("logo")]
        public string Logo { get; set; }

        [JsonPropertyName("flag")]
        public string Flag { get; set; }

        [JsonPropertyName("season")]
        public int Season { get; set; }

        [JsonPropertyName("round")]
        public string Round { get; set; }
    }

    public class TeamMatchInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("logo")]
        public string Logo { get; set; }

        [JsonPropertyName("winner")]
        public bool? Winner { get; set; }
    }

    public class TeamsInfo
    {
        [JsonPropertyName("home")]
        public TeamMatchInfo Home { get; set; }

        [JsonPropertyName("away")]
        public TeamMatchInfo Away { get; set; }
    }

    public class FixtureStatistics
    {
        [JsonPropertyName("team")]
        public TeamMatchInfo Team { get; set; }

        [JsonPropertyName("statistics")]
        public List<StatisticItem> Statistics { get; set; }
    }

    public class StatisticItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("value")]
        public object Value { get; set; }
    }

    public class FixtureEvent
    {
        [JsonPropertyName("time")]
        public EventTime Time { get; set; }

        [JsonPropertyName("team")]
        public TeamMatchInfo Team { get; set; }

        [JsonPropertyName("player")]
        public PlayerInfo Player { get; set; }

        [JsonPropertyName("assist")]
        public PlayerInfo Assist { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("detail")]
        public string Detail { get; set; }

        [JsonPropertyName("comments")]
        public string Comments { get; set; }
    }

    public class EventTime
    {
        [JsonPropertyName("elapsed")]
        public int Elapsed { get; set; }

        [JsonPropertyName("extra")]
        public int? Extra { get; set; }
    }

    public class PlayerInfo
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class FixtureLineup
    {
        [JsonPropertyName("team")]
        public TeamMatchInfo Team { get; set; }

        [JsonPropertyName("formation")]
        public string Formation { get; set; }

        [JsonPropertyName("startXI")]
        public List<PlayerLineup> StartXI { get; set; }

        [JsonPropertyName("substitutes")]
        public List<PlayerLineup> Substitutes { get; set; }

        [JsonPropertyName("coach")]
        public CoachInfo Coach { get; set; }
    }

    public class PlayerLineup
    {
        [JsonPropertyName("player")]
        public LineupPlayerInfo Player { get; set; }
    }

    public class LineupPlayerInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("pos")]
        public string Position { get; set; }

        [JsonPropertyName("grid")]
        public string Grid { get; set; }
    }

    public class CoachInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("photo")]
        public string Photo { get; set; }
    }

    public class FixturePlayer
    {
        [JsonPropertyName("team")]
        public TeamMatchInfo Team { get; set; }

        [JsonPropertyName("players")]
        public List<PlayerStatistics> Players { get; set; }
    }

    public class PlayerStatistics
    {
        [JsonPropertyName("player")]
        public PlayerInfo Player { get; set; }

        [JsonPropertyName("statistics")]
        public List<PlayerMatchStats> Statistics { get; set; }
    }

    public class PlayerMatchStats
    {
        [JsonPropertyName("games")]
        public PlayerGameStats Games { get; set; }

        [JsonPropertyName("offsides")]
        public int? Offsides { get; set; }

        [JsonPropertyName("shots")]
        public PlayerShotStats Shots { get; set; }

        [JsonPropertyName("goals")]
        public PlayerGoalStats Goals { get; set; }

        [JsonPropertyName("passes")]
        public PlayerPassStats Passes { get; set; }

        [JsonPropertyName("tackles")]
        public PlayerTackleStats Tackles { get; set; }

        [JsonPropertyName("duels")]
        public PlayerDuelStats Duels { get; set; }

        [JsonPropertyName("dribbles")]
        public PlayerDribbleStats Dribbles { get; set; }

        [JsonPropertyName("fouls")]
        public PlayerFoulStats Fouls { get; set; }

        [JsonPropertyName("cards")]
        public PlayerCardStats Cards { get; set; }

        [JsonPropertyName("penalty")]
        public PlayerPenaltyStats Penalty { get; set; }
    }

    public class PlayerGameStats
    {
        [JsonPropertyName("minutes")]
        public int? Minutes { get; set; }

        [JsonPropertyName("number")]
        public int? Number { get; set; }

        [JsonPropertyName("position")]
        public string Position { get; set; }

        [JsonPropertyName("rating")]
        public string Rating { get; set; }

        [JsonPropertyName("captain")]
        public bool Captain { get; set; }

        [JsonPropertyName("substitute")]
        public bool Substitute { get; set; }
    }

    public class PlayerShotStats
    {
        [JsonPropertyName("total")]
        public int? Total { get; set; }

        [JsonPropertyName("on")]
        public int? On { get; set; }
    }

    public class PlayerGoalStats
    {
        [JsonPropertyName("total")]
        public int? Total { get; set; }

        [JsonPropertyName("conceded")]
        public int? Conceded { get; set; }

        [JsonPropertyName("assists")]
        public int? Assists { get; set; }

        [JsonPropertyName("saves")]
        public int? Saves { get; set; }
    }

    public class PlayerPassStats
    {
        [JsonPropertyName("total")]
        public int? Total { get; set; }

        [JsonPropertyName("key")]
        public int? Key { get; set; }

        [JsonPropertyName("accuracy")]
        public string Accuracy { get; set; }
    }

    public class PlayerTackleStats
    {
        [JsonPropertyName("total")]
        public int? Total { get; set; }

        [JsonPropertyName("blocks")]
        public int? Blocks { get; set; }

        [JsonPropertyName("interceptions")]
        public int? Interceptions { get; set; }
    }

    public class PlayerDuelStats
    {
        [JsonPropertyName("total")]
        public int? Total { get; set; }

        [JsonPropertyName("won")]
        public int? Won { get; set; }
    }

    public class PlayerDribbleStats
    {
        [JsonPropertyName("attempts")]
        public int? Attempts { get; set; }

        [JsonPropertyName("success")]
        public int? Success { get; set; }

        [JsonPropertyName("past")]
        public int? Past { get; set; }
    }

    public class PlayerFoulStats
    {
        [JsonPropertyName("drawn")]
        public int? Drawn { get; set; }

        [JsonPropertyName("committed")]
        public int? Committed { get; set; }
    }

    public class PlayerCardStats
    {
        [JsonPropertyName("yellow")]
        public int? Yellow { get; set; }

        [JsonPropertyName("red")]
        public int? Red { get; set; }
    }

    public class PlayerPenaltyStats
    {
        [JsonPropertyName("won")]
        public int? Won { get; set; }

        [JsonPropertyName("committed")]
        public int? Committed { get; set; }

        [JsonPropertyName("scored")]
        public int? Scored { get; set; }

        [JsonPropertyName("missed")]
        public int? Missed { get; set; }

        [JsonPropertyName("saved")]
        public int? Saved { get; set; }
    }

    public class VenueInfo
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }
    }
}
