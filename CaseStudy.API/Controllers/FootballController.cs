using CaseStudy.Application.Interfaces;
using CaseStudy.Application.Models.BayTahmin;
using Microsoft.AspNetCore.Mvc;

namespace CaseStudy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FootballController : ControllerBase
    {
        private readonly IFootballDataService _footballService;
        private readonly ILogger<FootballController> _logger;

        public FootballController(IFootballDataService footballService, ILogger<FootballController> logger)
        {
            _footballService = footballService;
            _logger = logger;
        }

        [HttpGet("countries")]
        public async Task<ActionResult<List<CountryModel>>> GetCountries()
        {
            try
            {
                var countries = await _footballService.GetAvailableCountriesAsync();
                return Ok(countries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ülkeler getirilirken hata oluştu");
                return StatusCode(500, "Ülkeler getirilirken bir hata oluştu");
            }
        }

        [HttpGet("leagues/{countryCode}")]
        public async Task<ActionResult<List<LeagueModel>>> GetLeagues(string countryCode)
        {
            try
            {
                var leagues = await _footballService.GetLeaguesByCountryAsync(countryCode);
                return Ok(leagues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{CountryCode} ülkesinin ligleri getirilirken hata oluştu", countryCode);
                return StatusCode(500, $"{countryCode} ülkesinin ligleri getirilirken bir hata oluştu");
            }
        }

        [HttpGet("fixtures")]
        public async Task<ActionResult<List<Fixture>>> GetFixtures(
            [FromQuery] int leagueId,
            [FromQuery] int season,
            [FromQuery] string date)
        {
            try
            {
                var fixtures = await _footballService.GetFixturesByLeagueAndSeasonAsync(date);
                return Ok(fixtures);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Maçlar getirilirken hata oluştu. Lig: {LeagueId}, Sezon: {Season}, Tarih: {Date}", 
                    leagueId, season, date);
                return StatusCode(500, "Maçlar getirilirken bir hata oluştu");
            }
        }

        [HttpGet("premier-league-fixtures")]
        public async Task<ActionResult<List<Fixture>>> GetPremierLeagueFixtures([FromQuery] string? date = null)
        {
            try
            {
                // İngiltere'yi bul
                var countries = await _footballService.GetAvailableCountriesAsync();
                var england = countries.FirstOrDefault(c => c.Name.Equals("England", StringComparison.OrdinalIgnoreCase));

                if (england == null)
                {
                    return NotFound("İngiltere bulunamadı");
                }

                // Premier Lig'i bul
                var leagues = await _footballService.GetLeaguesByCountryAsync(england.Name);
                var premierLeague = leagues.FirstOrDefault(l => l.League.Name.Contains("Premier League", StringComparison.OrdinalIgnoreCase));

                if (premierLeague == null)
                {
                    return NotFound("Premier Lig bulunamadı");
                }

                _logger.LogInformation("İngiltere kodu: {Code}, Premier Lig ID: {Id}", england.Code, premierLeague.League.Id);

                // Eğer tarih belirtilmemişse bugünün tarihini kullan
                var targetDate = date ?? DateTime.Now.ToString("yyyy-MM-dd");
                var fixtures = await _footballService.GetFixturesByLeagueAndSeasonAsync(
                    date: targetDate
                );

                return Ok(fixtures);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Premier Lig maçları getirilirken hata oluştu");
                return StatusCode(500, "Premier Lig maçları getirilirken bir hata oluştu");
            }
        }

        [HttpGet("today-super-league")]
        public async Task<ActionResult<List<Fixture>>> GetTodaySuperLeagueFixtures()
        {
            try
            {
                // Türkiye'yi bul
                var countries = await _footballService.GetAvailableCountriesAsync();
                var turkey = countries.FirstOrDefault(c => c.Name.Equals("Turkey", StringComparison.OrdinalIgnoreCase));

                if (turkey == null)
                {
                    return NotFound("Türkiye bulunamadı");
                }

                // Süper Lig'i bul
                var leagues = await _footballService.GetLeaguesByCountryAsync(turkey.Name);
                var superLeague = leagues.FirstOrDefault(l => l.League.Name.Contains("Süper Lig", StringComparison.OrdinalIgnoreCase));

                if (superLeague == null)
                {
                    return NotFound("Süper Lig bulunamadı");
                }

                _logger.LogInformation("Türkiye kodu: {Code}, Süper Lig ID: {Id}", turkey.Code, superLeague.League.Id);

                // Bugünün maçlarını getir
                var today = DateTime.Now.ToString("yyyy-MM-dd");
                var fixtures = await _footballService.GetFixturesByLeagueAndSeasonAsync(
                    date: today
                );

                return Ok(fixtures);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Süper Lig'in bugünkü maçları getirilirken hata oluştu");
                return StatusCode(500, "Süper Lig'in bugünkü maçları getirilirken bir hata oluştu");
            }
        }
    }
}
