using CaseStudy.Application.Interfaces;
using CaseStudy.Application.Models.BayTahmin;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace CaseStudy.Application.Services
{
    public class BayTahminService : IBayTahminService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://api-football-v1.p.rapidapi.com/v3";

        public BayTahminService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = configuration["FootballApi:ApiKey"];
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
        }

        public async Task<IEnumerable<LeagueModel>> GetLeaguesAsync(string country = null)
        {
            var url = $"{_baseUrl}/leagues";
            if (!string.IsNullOrEmpty(country))
            {
                url += $"?country={country}";
            }

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<LeagueModel>>(content);

            return result.Response;
        }

        public async Task<LeagueModel> GetLeagueByIdAsync(int id)
        {
            var url = $"{_baseUrl}/leagues?id={id}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<LeagueModel>>(content);

            return result.Response.FirstOrDefault();
        }

        public async Task<IEnumerable<TeamModel>> GetTeamsByLeagueAsync(int leagueId)
        {
            var url = $"{_baseUrl}/teams?league={leagueId}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<TeamModel>>(content);

            return result.Response;
        }

        public async Task<TeamModel> GetTeamByIdAsync(int id)
        {
            var url = $"{_baseUrl}/teams?id={id}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<TeamModel>>(content);

            return result.Response.FirstOrDefault();
        }

        public async Task<IEnumerable<MatchModel>> GetMatchesByLeagueAsync(int leagueId)
        {
            var url = $"{_baseUrl}/fixtures?league={leagueId}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<MatchModel>>(content);

            return result.Response;
        }

        public async Task<IEnumerable<MatchModel>> GetUpcomingMatchesAsync(int leagueId)
        {
            var url = $"{_baseUrl}/fixtures?league={leagueId}&status=NS";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<MatchModel>>(content);

            return result.Response;
        }

        public async Task<MatchModel> GetMatchByIdAsync(int id)
        {
            var url = $"{_baseUrl}/fixtures?id={id}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<MatchModel>>(content);

            return result.Response.FirstOrDefault();
        }

        public async Task<MatchStatisticsModel> GetMatchStatisticsAsync(int matchId)
        {
            var url = $"{_baseUrl}/fixtures/statistics?fixture={matchId}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<MatchStatisticsModel>>(content);

            return result.Response.FirstOrDefault();
        }

        public async Task<IEnumerable<PlayerModel>> GetPlayersByTeamAsync(int teamId)
        {
            var url = $"{_baseUrl}/players/squads?team={teamId}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<PlayerModel>>(content);

            return result.Response;
        }

        public async Task<PlayerModel> GetPlayerByIdAsync(int id)
        {
            var url = $"{_baseUrl}/players?id={id}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<PlayerModel>>(content);

            return result.Response.FirstOrDefault();
        }

        public async Task<IEnumerable<PredictionModel>> GetPredictionsByMatchAsync(int matchId)
        {
            var url = $"{_baseUrl}/predictions?fixture={matchId}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<PredictionModel>>(content);

            return result.Response;
        }

        public async Task<PredictionModel> CreatePredictionAsync(PredictionModel prediction)
        {
            // Bu metod API'de yok, kendi veritabanımıza kaydetmemiz gerekiyor
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<OddsModel>> GetOddsByMatchAsync(int matchId)
        {
            var url = $"{_baseUrl}/odds?fixture={matchId}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<OddsModel>>(content);

            return result.Response;
        }

        public async Task<OddsModel> UpdateOddsAsync(int matchId, OddsModel odds)
        {
            // Bu metod API'de yok, kendi veritabanımıza kaydetmemiz gerekiyor
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TransferModel>> GetTransfersByPlayerAsync(int playerId)
        {
            var url = $"{_baseUrl}/transfers?player={playerId}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<TransferModel>>(content);

            return result.Response;
        }

        public async Task<IEnumerable<TransferModel>> GetTransfersByTeamAsync(int teamId)
        {
            var url = $"{_baseUrl}/transfers?team={teamId}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<TransferModel>>(content);

            return result.Response;
        }

        public async Task<IEnumerable<InjuryModel>> GetInjuriesByPlayerAsync(int playerId)
        {
            var url = $"{_baseUrl}/injuries?player={playerId}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<InjuryModel>>(content);

            return result.Response;
        }

        public async Task<IEnumerable<InjuryModel>> GetActiveInjuriesByTeamAsync(int teamId)
        {
            var url = $"{_baseUrl}/injuries?team={teamId}&status=active";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<InjuryModel>>(content);

            return result.Response;
        }

        public async Task SyncLeagueDataAsync(int leagueId, int season)
        {
            // API'den veriyi çekip veritabanına kaydetme işlemi
            var leagues = await GetLeaguesAsync();
            // Veritabanına kaydetme işlemi
        }

        public async Task SyncTeamDataAsync(int leagueId)
        {
            // API'den veriyi çekip veritabanına kaydetme işlemi
            var teams = await GetTeamsByLeagueAsync(leagueId);
            // Veritabanına kaydetme işlemi
        }

        public async Task SyncFixturesAsync(int leagueId, int season)
        {
            // API'den veriyi çekip veritabanına kaydetme işlemi
            var matches = await GetMatchesByLeagueAsync(leagueId);
            // Veritabanına kaydetme işlemi
        }

        public async Task SyncLiveScoresAsync(int leagueId)
        {
            var url = $"{_baseUrl}/fixtures?league={leagueId}&live=all";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<MatchModel>>(content);

            // Canlı skorları veritabanına kaydetme işlemi
        }

        public async Task UpdateMatchStatisticsAsync(int matchId)
        {
            var statistics = await GetMatchStatisticsAsync(matchId);
            // Veritabanına kaydetme işlemi
        }

        public async Task UpdateTeamStatisticsAsync(int teamId, int leagueId, int season)
        {
            var url = $"{_baseUrl}/teams/statistics?team={teamId}&league={leagueId}&season={season}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            // Takım istatistiklerini veritabanına kaydetme işlemi
        }

        private class ApiResponse<T>
        {
            public List<T> Response { get; set; }
        }
    }
}
