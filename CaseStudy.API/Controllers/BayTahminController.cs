using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CaseStudy.Application.Services;
using Microsoft.Extensions.Logging;

namespace CaseStudy.API.Controllers
{
    [Route("core/api/[controller]/[action]")]
    [ApiController]
    [AllowAnonymous]
    public class BayTahminController : ControllerBase
    {
        private readonly IBayTahminService _bayTahminService;
        private readonly ILogger<BayTahminController> _logger;

        public BayTahminController(
            IBayTahminService bayTahminService,
            ILogger<BayTahminController> logger)
        {
            _bayTahminService = bayTahminService;
            _logger = logger;
        }

        // 1. Sezonları Getir
        [HttpGet]
        public async Task<IActionResult> GetSeasons()
        {
            try
            {
                var seasons = await _bayTahminService.GetSeasonsAsync();
                return Ok(seasons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sezonları çekerken bir hata oluştu.");
                return StatusCode(500, "Sezon verileri alınamadı.");
            }
        }

        // 2. Ülkeleri Getir
        [HttpGet]
        public async Task<IActionResult> GetCountries()
        {
            try
            {
                var countries = await _bayTahminService.GetCountriesAsync();
                return Ok(countries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ülkeleri çekerken bir hata oluştu.");
                return StatusCode(500, "Ülke verileri alınamadı.");
            }
        }

        // 3. Ligleri Getir
        [HttpGet("{season}/{country}")]
        public async Task<IActionResult> GetLeagues(int season, string country)
        {
            try
            {
                var leagues = await _bayTahminService.GetLeaguesAsync(season, country);
                return Ok(leagues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ligleri çekerken bir hata oluştu.");
                return StatusCode(500, "Lig verileri alınamadı.");
            }
        }

        // 4. Takımları Getir
        [HttpGet("{leagueId}/{season}")]
        public async Task<IActionResult> GetTeams(int leagueId, int season)
        {
            try
            {
                var teams = await _bayTahminService.GetTeamsAsync(leagueId, season);
                return Ok(teams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Takımları çekerken bir hata oluştu.");
                return StatusCode(500, "Takım verileri alınamadı.");
            }
        }

        // 5. Takım İstatistiklerini Getir
        [HttpGet("{teamId}/{leagueId}/{season}")]
        public async Task<IActionResult> GetTeamStatistics(int teamId, int leagueId, int season)
        {
            try
            {
                var stats = await _bayTahminService.GetTeamStatisticsAsync(teamId, leagueId, season);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Takım istatistiklerini çekerken bir hata oluştu.");
                return StatusCode(500, "Takım istatistik verileri alınamadı.");
            }
        }

        // 6. Maçları Getir
        [HttpGet("{leagueId}/{season}")]
        public async Task<IActionResult> GetGames(int leagueId, int season)
        {
            try
            {
                var games = await _bayTahminService.GetGamesAsync(leagueId, season);
                return Ok(games);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Maçları çekerken bir hata oluştu.");
                return StatusCode(500, "Maç verileri alınamadı.");
            }
        }

        // 7. H2H (Head to Head) Bilgileri Getir
        [HttpGet("{team1Id}/{team2Id}")]
        public async Task<IActionResult> GetHeadToHead(int team1Id, int team2Id)
        {
            try
            {
                var h2h = await _bayTahminService.GetHeadToHeadAsync(team1Id, team2Id);
                return Ok(h2h);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "H2H bilgilerini çekerken bir hata oluştu.");
                return StatusCode(500, "H2H verileri alınamadı.");
            }
        }

        // 8. Lig Sıralamalarını Getir
        [HttpGet("{leagueId}/{season}")]
        public async Task<IActionResult> GetStandings(int leagueId, int season)
        {
            try
            {
                var standings = await _bayTahminService.GetStandingsAsync(leagueId, season);
                return Ok(standings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lig sıralamalarını çekerken bir hata oluştu.");
                return StatusCode(500, "Sıralama verileri alınamadı.");
            }
        }

        // 9. Bahis Oranlarını Getir
        [HttpGet("{gameId}")]
        public async Task<IActionResult> GetOdds(int gameId)
        {
            try
            {
                var odds = await _bayTahminService.GetOddsAsync(gameId);
                return Ok(odds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bahis oranlarını çekerken bir hata oluştu.");
                return StatusCode(500, "Bahis oranı verileri alınamadı.");
            }
        }

        // 10. Bahisleri Getir
        [HttpGet("{gameId}")]
        public async Task<IActionResult> GetBets(int gameId)
        {
            try
            {
                var bets = await _bayTahminService.GetBetsAsync(gameId);
                return Ok(bets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bahis bilgilerini çekerken bir hata oluştu.");
                return StatusCode(500, "Bahis verileri alınamadı.");
            }
        }
    }
}
