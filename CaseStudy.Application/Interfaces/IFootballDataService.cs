using CaseStudy.Application.Models.BayTahmin;

namespace CaseStudy.Application.Interfaces
{
    public interface IFootballDataService
    {
        Task<List<CountryModel>> GetAvailableCountriesAsync();
        Task<List<LeagueModel>> GetLeaguesByCountryAsync(string countryCode);
        Task<List<Fixture>> GetFixturesByLeagueAndSeasonAsync(int leagueId, int season, string date);
    }
}
