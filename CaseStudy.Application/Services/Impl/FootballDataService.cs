using CaseStudy.Application.Interfaces;
using CaseStudy.Application.Models.BayTahmin;
using Microsoft.Extensions.Logging;

namespace CaseStudy.Application.Services.Impl
{
    public class FootballDataService : IFootballDataService
    {
        private readonly IBayTahminService _bayTahminService;
        private readonly ILogger<FootballDataService> _logger;

        public FootballDataService(IBayTahminService bayTahminService, ILogger<FootballDataService> logger)
        {
            _bayTahminService = bayTahminService ?? throw new ArgumentNullException(nameof(bayTahminService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<CountryModel>> GetAvailableCountriesAsync()
        {
            try
            {
                _logger.LogInformation("Mevcut ülkeler getiriliyor");
                return await _bayTahminService.GetCountriesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ülkeleri getirirken hata oluştu");
                throw;
            }
        }

        public async Task<List<LeagueModel>> GetLeaguesByCountryAsync(string countryCode)
        {
            try
            {
                _logger.LogInformation("{CountryCode} ülkesinin ligleri getiriliyor", countryCode);
                return await _bayTahminService.GetLeaguesAsync(countryCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{CountryCode} ülkesinin ligleri getirilirken hata oluştu", countryCode);
                throw;
            }
        }

        public async Task<List<Fixture>> GetFixturesByLeagueAndSeasonAsync(string date)
        {
            try
            {
                _logger.LogInformation("Lig {LeagueId}, sezon {Season}, tarih {Date} için maçlar getiriliyor",  date);
                return await _bayTahminService.GetFixturesAsync(date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Maçlar getirilirken hata oluştu. Lig: {LeagueId}, Sezon: {Season}, Tarih: {Date}",  date);
                throw;
            }
        }
    }
}
