using CaseStudy.Application.Models.BayTahmin;

namespace CaseStudy.Application.Interfaces
{
    public interface IBayTahminService
    {
        Task<IEnumerable<LeagueModel>> GetLeaguesAsync(string country = null);
        Task<LeagueModel> GetLeagueByIdAsync(int id);
        
        Task<IEnumerable<TeamModel>> GetTeamsByLeagueAsync(int leagueId);
        Task<TeamModel> GetTeamByIdAsync(int id);
        
        Task<IEnumerable<MatchModel>> GetMatchesByLeagueAsync(int leagueId);
        Task<IEnumerable<MatchModel>> GetUpcomingMatchesAsync(int leagueId);
        Task<MatchModel> GetMatchByIdAsync(int id);
        
        Task<MatchStatisticsModel> GetMatchStatisticsAsync(int matchId);
        
        Task<IEnumerable<PlayerModel>> GetPlayersByTeamAsync(int teamId);
        Task<PlayerModel> GetPlayerByIdAsync(int id);
        
        Task<IEnumerable<PredictionModel>> GetPredictionsByMatchAsync(int matchId);
        Task<PredictionModel> CreatePredictionAsync(PredictionModel prediction);
        
        Task<IEnumerable<OddsModel>> GetOddsByMatchAsync(int matchId);
        Task<OddsModel> UpdateOddsAsync(int matchId, OddsModel odds);
        
        Task<IEnumerable<TransferModel>> GetTransfersByPlayerAsync(int playerId);
        Task<IEnumerable<TransferModel>> GetTransfersByTeamAsync(int teamId);
        
        Task<IEnumerable<InjuryModel>> GetInjuriesByPlayerAsync(int playerId);
        Task<IEnumerable<InjuryModel>> GetActiveInjuriesByTeamAsync(int teamId);

        // API Football özel metodları
        Task SyncLeagueDataAsync(int leagueId, int season);
        Task SyncTeamDataAsync(int leagueId);
        Task SyncFixturesAsync(int leagueId, int season);
        Task SyncLiveScoresAsync(int leagueId);
        Task UpdateMatchStatisticsAsync(int matchId);
        Task UpdateTeamStatisticsAsync(int teamId, int leagueId, int season);
    }
}
