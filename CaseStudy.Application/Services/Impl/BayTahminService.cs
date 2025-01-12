using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CaseStudy.Application.Services.Impl
{
    public class BayTahminService : IBayTahminService
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "0e63c874eae037a8d315c90fd79a3281";

        public BayTahminService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
            _httpClient.BaseAddress = new Uri("https://v1.basketball.api-sports.io/");
        }

        // 1. Seasons ve Countries
        public async Task<List<dynamic>> GetSeasonsAsync()
        {
            var response = await _httpClient.GetAsync("seasons");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<dynamic>>();
        }

        public async Task<List<dynamic>> GetCountriesAsync()
        {
            var response = await _httpClient.GetAsync("countries");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<dynamic>>();
        }

        // 2. Leagues
        public async Task<List<dynamic>> GetLeaguesAsync(int season, string country)
        {
            var response = await _httpClient.GetAsync($"leagues?season={season}&country={country}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<dynamic>>();
        }

        // 3. Teams
        public async Task<List<dynamic>> GetTeamsAsync(int leagueId, int season)
        {
            var response = await _httpClient.GetAsync($"teams?league={leagueId}&season={season}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<dynamic>>();
        }

        public async Task<dynamic> GetTeamStatisticsAsync(int teamId, int leagueId, int season)
        {
            var response = await _httpClient.GetAsync($"teams/statistics?team={teamId}&league={leagueId}&season={season}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<dynamic>();
        }

        // 4. Players
        public async Task<List<dynamic>> GetPlayersAsync(int teamId, int season)
        {
            var response = await _httpClient.GetAsync($"players?team={teamId}&season={season}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<dynamic>>();
        }

        public async Task<dynamic> GetPlayerStatisticsAsync(int playerId, int season)
        {
            var response = await _httpClient.GetAsync($"players/statistics?player={playerId}&season={season}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<dynamic>();
        }

        // 5. Games
        public async Task<List<dynamic>> GetGamesAsync(int leagueId, int season)
        {
            var response = await _httpClient.GetAsync($"games?league={leagueId}&season={season}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<dynamic>>();
        }

        public async Task<List<dynamic>> GetHeadToHeadAsync(int team1Id, int team2Id)
        {
            var response = await _httpClient.GetAsync($"games/h2h?team1={team1Id}&team2={team2Id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<dynamic>>();
        }

        // 6. Standings
        public async Task<List<dynamic>> GetStandingsAsync(int leagueId, int season)
        {
            var response = await _httpClient.GetAsync($"standings?league={leagueId}&season={season}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<dynamic>>();
        }

        // 7. Odds ve Bahis
        public async Task<List<dynamic>> GetOddsAsync(int gameId)
        {
            var response = await _httpClient.GetAsync($"odds?game={gameId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<dynamic>>();
        }

        public async Task<List<dynamic>> GetBetsAsync(int gameId)
        {
            var response = await _httpClient.GetAsync($"bets?game={gameId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<dynamic>>();
        }
    }
}
