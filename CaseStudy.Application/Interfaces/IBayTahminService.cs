using CaseStudy.Application.Models.BayTahmin;

namespace CaseStudy.Application.Interfaces
{
    public interface IBayTahminService
    {
        // Countries
        Task<List<CountryModel>> GetCountriesAsync();
        Task<CountryModel> GetCountryByCodeAsync(string code);

        // Leagues
        Task<List<LeagueModel>> GetLeaguesAsync(string country = null);
        Task<LeagueModel> GetLeagueByIdAsync(int id);
        Task<List<int>> GetLeagueSeasonsAsync();

        // Teams
        Task<TeamModel> GetTeamByIdAsync(int teamId);
        Task<TeamStatistics> GetTeamStatisticsAsync(int teamId, int leagueId, int season);
        Task<List<int>> GetTeamSeasonsAsync(int teamId);
        Task<List<string>> GetTeamCountriesAsync();

        // Venues
        Task<VenueModel> GetVenueByIdAsync(int venueId);
        Task<List<VenueModel>> GetVenuesBySearchAsync(string name = null, string city = null, string country = null);

        // Standings
        Task<StandingsResponse> GetLeagueStandingsAsync(int leagueId, int season);

        // Fixtures
        Task<List<string>> GetFixtureRoundsAsync(int leagueId, int season);
        Task<List<Fixture>> GetFixturesAsync(string date);
        Task<List<Fixture>> GetHeadToHeadFixturesAsync(string h2h);
        Task<List<FixtureStatistics>> GetFixtureStatisticsAsync(int fixtureId, int? teamId = null);
        Task<List<FixtureEvent>> GetFixtureEventsAsync(int fixtureId);
        Task<List<FixtureLineup>> GetFixtureLineupsAsync(int fixtureId);
        Task<List<FixturePlayer>> GetFixturePlayersAsync(int fixtureId);

        // Injuries
        Task<List<InjuryModel>> GetFixtureInjuriesAsync(int fixtureId);
        Task<List<InjuryModel>> GetTeamInjuriesAsync(int teamId, int? leagueId = null, int? season = null);
        Task<List<InjuryModel>> GetPlayerInjuriesAsync(int playerId);

        // Players
        Task<List<int>> GetPlayerSeasonsAsync();
        Task<PlayerDetailedInfo> GetPlayerProfileAsync(int playerId);
        Task<List<PlayerProfile>> GetPlayerStatisticsAsync(int playerId, int season);
        Task<List<PlayerSquad>> GetTeamSquadAsync(int teamId);
        Task<List<PlayerTeam>> GetPlayerTeamsAsync(int playerId);
        Task<List<TopPlayer>> GetTopAssistsAsync(int leagueId, int season);
        Task<List<TopPlayer>> GetTopYellowCardsAsync(int leagueId, int season);
        Task<List<TopPlayer>> GetTopRedCardsAsync(int leagueId, int season);

        // Transfers
        Task<List<TransferModel>> GetPlayerTransfersAsync(int playerId);

        // Trophies
        Task<List<TrophyModel>> GetPlayerTrophiesAsync(int playerId);

        // Sidelined
        Task<List<SidelinedModel>> GetPlayerSidelinedAsync(int playerId);

        // Odds
        Task<List<OddsModel>> GetFixtureOddsAsync(int fixtureId, int? bookmaker = null, int? bet = null);
        Task<List<OddsMapping>> GetOddsMappingAsync();
        Task<List<BookmakerInfo>> GetBookmakersAsync();
        Task<List<BetInfo>> GetBetsAsync();
        Task<Fixture> GetMatchByIdAsync(int matchId);
    }
}
