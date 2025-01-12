using CaseStudy.Application.Models; // Gerekli modeller burada tanımlı
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CaseStudy.Application.Services
{
    public interface IBayTahminService
    {
        // 1. Sezonları Getir
        Task<List<SeasonModel>> GetSeasonsAsync();

        // 2. Ülkeleri Getir
        Task<List<CountryModel>> GetCountriesAsync();

        // 3. Ligleri Getir
        Task<List<LeagueModel>> GetLeaguesAsync(int season, string country);

        // 4. Takımları Getir
        Task<List<TeamModel>> GetTeamsAsync(int leagueId, int season);

        // 5. Takım İstatistiklerini Getir
        Task<TeamStatisticsModel> GetTeamStatisticsAsync(int teamId, int leagueId, int season);

        // 6. Maçları Getir
        Task<List<GameModel>> GetGamesAsync(int leagueId, int season);

        // 7. H2H (Head to Head) Bilgileri Getir
        Task<H2HModel> GetHeadToHeadAsync(int team1Id, int team2Id);

        // 8. Lig Sıralamalarını Getir
        Task<StandingsModel> GetStandingsAsync(int leagueId, int season);

        // 9. Bahis Oranlarını Getir
        Task<OddsModel> GetOddsAsync(int gameId);

        // 10. Bahisleri Getir
        Task<List<BetModel>> GetBetsAsync(int gameId);
    }
}
