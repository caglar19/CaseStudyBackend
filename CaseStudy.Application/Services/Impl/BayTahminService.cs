using CaseStudy.Application.Interfaces;
using CaseStudy.Application.Models.BayTahmin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace CaseStudy.Application.Services.Impl
{
    public class BayTahminService : IBayTahminService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BayTahminService> _logger;
        private readonly string _baseUrl = "https://v3.football.api-sports.io";

        public BayTahminService(IConfiguration configuration, HttpClient httpClient, ILogger<BayTahminService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var apiKey = configuration["FootballApi:ApiKey"] 
                ?? throw new ArgumentException("FootballApi:ApiKey configuration is missing");

            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", "v3.football.api-sports.io");
        }

        private async Task<ApiResponse<T>> GetApiResponseAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null)
        {
            try
            {
                var url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
                if (queryParams?.Any() == true)
                {
                    var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value)}"));
                    url += $"?{queryString}";
                }

                _logger.LogInformation("Making API request to: {Url}", url);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API Response Content: {Content}", content);

                var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    _logger.LogWarning("API response was null for endpoint: {Endpoint}", endpoint);
                    throw new InvalidOperationException($"API response was null for endpoint: {endpoint}");
                }

                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization failed for endpoint {Endpoint}: {Message}", endpoint, ex.Message);
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for endpoint {Endpoint}: {Message}", endpoint, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request for endpoint {Endpoint}: {Message}", endpoint, ex.Message);
                throw;
            }
        }

        private async Task<List<T>> GetApiListResponseAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null)
        {
            var result = await GetApiResponseAsync<T>(endpoint, queryParams);
            return result.Response ?? new List<T>();
        }

        #region Countries
        public async Task<List<CountryModel>> GetCountriesAsync()
        {
            var result = await GetApiResponseAsync<CountryModel>("countries");
            return result.Response ?? new List<CountryModel>();
        }

        public async Task<CountryModel> GetCountryByCodeAsync(string code)
        {
            var result = await GetApiResponseAsync<CountryModel>("countries", new Dictionary<string, string> { { "code", code } });
            return result.Response?.FirstOrDefault();
        }
        #endregion

        #region Leagues
        public Task<List<LeagueModel>> GetLeaguesAsync(string country)
        {
            var queryParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(country))
            {
                queryParams.Add("country", country);
            }
            return GetApiListResponseAsync<LeagueModel>("leagues", queryParams);
        }

        public async Task<LeagueModel> GetLeagueByIdAsync(int id)
            => (await GetApiResponseAsync<LeagueModel>("leagues", new Dictionary<string, string> { { "id", id.ToString() } })).Response?.FirstOrDefault();

        public async Task<List<int>> GetLeagueSeasonsAsync()
        {
            var response = await GetApiResponseAsync<int>("leagues/seasons");
            return response.Response ?? new List<int>();
        }

        public async Task<TeamModel> GetTeamByIdAsync(int teamId)
        {
            var response = await GetApiResponseAsync<TeamModel>("teams", new Dictionary<string, string> { { "id", teamId.ToString() } });
            return response.Response?.FirstOrDefault() ?? throw new InvalidOperationException("Team not found");
        }

        public async Task<TeamStatistics> GetTeamStatisticsAsync(int teamId, int leagueId, int season)
            => (await GetApiResponseAsync<TeamStatistics>("teams/statistics", 
                new Dictionary<string, string>
                {
                    { "team", teamId.ToString() },
                    { "league", leagueId.ToString() },
                    { "season", season.ToString() }
                })).Response?.FirstOrDefault();

        public Task<List<int>> GetTeamSeasonsAsync(int teamId)
            => GetApiListResponseAsync<int>("teams/seasons", new Dictionary<string, string> { { "team", teamId.ToString() } });

        public Task<List<string>> GetTeamCountriesAsync()
            => GetApiListResponseAsync<string>("teams/countries");
        #endregion

        #region Venues
        public async Task<VenueModel> GetVenueByIdAsync(int venueId)
            => (await GetApiResponseAsync<VenueModel>("venues", new Dictionary<string, string> { { "id", venueId.ToString() } })).Response?.FirstOrDefault();

        public Task<List<VenueModel>> GetVenuesBySearchAsync(string name = null, string city = null, string country = null)
        {
            var queryParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(name)) queryParams.Add("name", name);
            if (!string.IsNullOrEmpty(city)) queryParams.Add("city", city);
            if (!string.IsNullOrEmpty(country)) queryParams.Add("country", country);
            
            return GetApiListResponseAsync<VenueModel>("venues", queryParams);
        }
        #endregion

        #region Standings
        public async Task<StandingsResponse> GetLeagueStandingsAsync(int leagueId, int season)
            => (await GetApiResponseAsync<StandingsResponse>("standings", 
                new Dictionary<string, string>
                {
                    { "league", leagueId.ToString() },
                    { "season", season.ToString() }
                })).Response?.FirstOrDefault();
        #endregion

        #region Fixtures
        public Task<List<string>> GetFixtureRoundsAsync(int leagueId, int season)
            => GetApiListResponseAsync<string>("fixtures/rounds", 
                new Dictionary<string, string>
                {
                    { "league", leagueId.ToString() },
                    { "season", season.ToString() }
                });

        public async Task<List<Fixture>> GetFixturesAsync(string date)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "status", "NS" },
                { "date", date }
            };

            return await GetApiListResponseAsync<Fixture>("fixtures", queryParams);
        }

        public Task<List<Fixture>> GetHeadToHeadFixturesAsync(string h2h)
            => GetApiListResponseAsync<Fixture>("fixtures/headtohead", new Dictionary<string, string> { { "h2h", h2h } });

        public async Task<Fixture> GetMatchByIdAsync(int matchId)
        {
            var response = await GetApiResponseAsync<Fixture>("fixtures", new Dictionary<string, string> { { "id", matchId.ToString() } });
            return response.Response?.FirstOrDefault() ?? throw new InvalidOperationException("Match not found");
        }

        public Task<List<FixtureStatistics>> GetFixtureStatisticsAsync(int fixtureId)
        {
            var queryParams = new Dictionary<string, string> { { "fixture", fixtureId.ToString() } };
            
            return GetApiListResponseAsync<FixtureStatistics>("fixtures/statistics", queryParams);
        }

        public Task<List<FixtureEvent>> GetFixtureEventsAsync(int fixtureId)
            => GetApiListResponseAsync<FixtureEvent>("fixtures/events", new Dictionary<string, string> { { "fixture", fixtureId.ToString() } });

        public Task<List<FixtureLineup>> GetFixtureLineupsAsync(int fixtureId)
            => GetApiListResponseAsync<FixtureLineup>("fixtures/lineups", new Dictionary<string, string> { { "fixture", fixtureId.ToString() } });

        public Task<List<FixturePlayer>> GetFixturePlayersAsync(int fixtureId)
            => GetApiListResponseAsync<FixturePlayer>("fixtures/players", new Dictionary<string, string> { { "fixture", fixtureId.ToString() } });
        #endregion

        #region Injuries
        public Task<List<InjuryModel>> GetFixtureInjuriesAsync(int fixtureId)
            => GetApiListResponseAsync<InjuryModel>("injuries", new Dictionary<string, string> { { "fixture", fixtureId.ToString() } });

        public Task<List<InjuryModel>> GetTeamInjuriesAsync(int teamId, int? leagueId = null, int? season = null)
        {
            var queryParams = new Dictionary<string, string> { { "team", teamId.ToString() } };
            if (leagueId.HasValue) queryParams.Add("league", leagueId.ToString());
            if (season.HasValue) queryParams.Add("season", season.ToString());
            
            return GetApiListResponseAsync<InjuryModel>("injuries", queryParams);
        }

        public Task<List<InjuryModel>> GetPlayerInjuriesAsync(int playerId)
            => GetApiListResponseAsync<InjuryModel>("injuries", new Dictionary<string, string> { { "player", playerId.ToString() } });
        #endregion

        #region Players
        public Task<List<int>> GetPlayerSeasonsAsync()
            => GetApiListResponseAsync<int>("players/seasons");

        public async Task<PlayerDetailedInfo> GetPlayerProfileAsync(int playerId)
            => (await GetApiResponseAsync<PlayerDetailedInfo>("players", new Dictionary<string, string> { { "id", playerId.ToString() } })).Response?.FirstOrDefault();

        public Task<List<PlayerProfile>> GetPlayerStatisticsAsync(int playerId, int season)
            => GetApiListResponseAsync<PlayerProfile>("players", 
                new Dictionary<string, string>
                {
                    { "id", playerId.ToString() },
                    { "season", season.ToString() }
                });

        public Task<List<PlayerSquad>> GetTeamSquadAsync(int teamId)
            => GetApiListResponseAsync<PlayerSquad>("players/squads", new Dictionary<string, string> { { "team", teamId.ToString() } });

        public Task<List<PlayerTeam>> GetPlayerTeamsAsync(int playerId)
            => GetApiListResponseAsync<PlayerTeam>("players/teams", new Dictionary<string, string> { { "player", playerId.ToString() } });

        public Task<List<TopPlayer>> GetTopAssistsAsync(int leagueId, int season)
            => GetApiListResponseAsync<TopPlayer>("players/topassists", 
                new Dictionary<string, string>
                {
                    { "league", leagueId.ToString() },
                    { "season", season.ToString() }
                });

        public Task<List<TopPlayer>> GetTopYellowCardsAsync(int leagueId, int season)
            => GetApiListResponseAsync<TopPlayer>("players/topyellowcards", 
                new Dictionary<string, string>
                {
                    { "league", leagueId.ToString() },
                    { "season", season.ToString() }
                });

        public Task<List<TopPlayer>> GetTopRedCardsAsync(int leagueId, int season)
            => GetApiListResponseAsync<TopPlayer>("players/topredcards", 
                new Dictionary<string, string>
                {
                    { "league", leagueId.ToString() },
                    { "season", season.ToString() }
                });
        #endregion

        #region Transfers & Trophies
        public Task<List<TransferModel>> GetPlayerTransfersAsync(int playerId)
            => GetApiListResponseAsync<TransferModel>("transfers", new Dictionary<string, string> { { "player", playerId.ToString() } });

        public Task<List<TrophyModel>> GetPlayerTrophiesAsync(int playerId)
            => GetApiListResponseAsync<TrophyModel>("trophies", new Dictionary<string, string> { { "player", playerId.ToString() } });

        public Task<List<SidelinedModel>> GetPlayerSidelinedAsync(int playerId)
            => GetApiListResponseAsync<SidelinedModel>("sidelined", new Dictionary<string, string> { { "player", playerId.ToString() } });
        #endregion

        #region Odds
        public Task<List<OddsModel>> GetFixtureOddsAsync(int fixtureId, int? bookmaker = null, int? bet = null)
        {
            var queryParams = new Dictionary<string, string> { { "fixture", fixtureId.ToString() } };
            if (bookmaker.HasValue) queryParams.Add("bookmaker", bookmaker.ToString());
            if (bet.HasValue) queryParams.Add("bet", bet.ToString());
            
            return GetApiListResponseAsync<OddsModel>("odds", queryParams);
        }

        public Task<List<OddsMapping>> GetOddsMappingAsync()
            => GetApiListResponseAsync<OddsMapping>("odds/mapping");

        public Task<List<BookmakerInfo>> GetBookmakersAsync()
            => GetApiListResponseAsync<BookmakerInfo>("odds/bookmakers");

        public async Task<List<BetInfo>> GetBetsAsync()
        {
            var response = await GetApiResponseAsync<BetInfo>("odds/bets");
            return response.Response ?? new List<BetInfo>();
        }
        #endregion

        #region Predictions
        public async Task<Prediction> GetPredictionAsync(int fixtureId)
        {
            var response = await GetApiResponseAsync<Prediction>("predictions", new Dictionary<string, string> { { "fixture", fixtureId.ToString() } });
            return response.Response.FirstOrDefault();
        }
        #endregion

        private class ApiResponse<T>
        {
            [JsonPropertyName("response")]
            public List<T> Response { get; set; }

            [JsonPropertyName("errors")]
            public object Errors { get; set; }
        }

        private class SeasonResponse
        {
            [JsonPropertyName("response")]
            public List<int> Response { get; set; }

            [JsonPropertyName("errors")]
            public object Errors { get; set; }
        }
    }
}
