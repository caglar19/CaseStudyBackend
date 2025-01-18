namespace CaseStudy.Application.Constants
{
    public static class ApiConstants
    {
        public const string BaseUrl = "https://v3.football.api-sports.io";
        public const string RapidApiHostHeader = "x-rapidapi-host";
        public const string RapidApiKeyHeader = "x-rapidapi-key";
        public const string RapidApiHost = "v3.football.api-sports.io";
        
        public static class Endpoints
        {
            public const string Countries = "/countries";
            public const string Leagues = "/leagues";
            public const string LeagueSeasons = "/leagues/seasons";
            public const string Teams = "/teams";
            public const string TeamStatistics = "/teams/statistics";
            public const string TeamSeasons = "/teams/seasons";
            public const string TeamCountries = "/teams/countries";
            public const string Venues = "/venues";
            public const string Standings = "/standings";
            public const string FixtureRounds = "/fixtures/rounds";
            public const string Fixtures = "/fixtures";
            public const string HeadToHead = "/fixtures/headtohead";
            public const string FixtureStatistics = "/fixtures/statistics";
            public const string FixtureEvents = "/fixtures/events";
            public const string FixtureLineups = "/fixtures/lineups";
            public const string FixturePlayers = "/fixtures/players";
            public const string Injuries = "/injuries";
            public const string PlayerSeasons = "/players/seasons";
            public const string PlayerProfiles = "/players/profiles";
            public const string Players = "/players";
            public const string PlayerSquads = "/players/squads";
            public const string PlayerTeams = "/players/teams";
            public const string TopAssists = "/players/topassists";
            public const string TopYellowCards = "/players/topyellowcards";
            public const string TopRedCards = "/players/topredcards";
            public const string Transfers = "/transfers";
            public const string Trophies = "/trophies";
            public const string Sidelined = "/sidelined";
            public const string Odds = "/odds";
            public const string OddsMapping = "/odds/mapping";
            public const string Bookmakers = "/odds/bookmakers";
            public const string Bets = "/odds/bets";
        }
    }
}
