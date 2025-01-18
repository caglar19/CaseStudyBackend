using CaseStudy.Application.Interfaces;
using CaseStudy.Application.Models.BayTahmin;
using Microsoft.AspNetCore.Mvc;

namespace CaseStudy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BayTahminController : ControllerBase
    {
        private readonly IBayTahminService _bayTahminService;
        private readonly ILogger<BayTahminController> _logger;

        public BayTahminController(IBayTahminService bayTahminService, ILogger<BayTahminController> logger)
        {
            _bayTahminService = bayTahminService ?? throw new ArgumentNullException(nameof(bayTahminService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Countries
        [HttpGet("countries")]
        [ProducesResponseType(typeof(IEnumerable<CountryModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CountryModel>>> GetCountries()
        {
            var countries = await _bayTahminService.GetCountriesAsync();
            return Ok(countries);
        }

        [HttpGet("countries/{code}")]
        [ProducesResponseType(typeof(CountryModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CountryModel>> GetCountryByCode(string code)
        {
            var country = await _bayTahminService.GetCountryByCodeAsync(code);
            if (country == null)
                return NotFound();
            return Ok(country);
        }
        #endregion

        #region Leagues
        [HttpGet("leagues")]
        [ProducesResponseType(typeof(IEnumerable<LeagueModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LeagueModel>>> GetLeagues([FromQuery] string country = null)
        {
            var leagues = await _bayTahminService.GetLeaguesAsync(country);
            return Ok(leagues);
        }

        [HttpGet("leagues/{id}")]
        [ProducesResponseType(typeof(LeagueModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LeagueModel>> GetLeague(int id)
        {
            var league = await _bayTahminService.GetLeagueByIdAsync(id);
            if (league == null)
                return NotFound();
            return Ok(league);
        }

        [HttpGet("leagues/seasons")]
        [ProducesResponseType(typeof(IEnumerable<int>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<int>>> GetLeagueSeasons()
        {
            var seasons = await _bayTahminService.GetLeagueSeasonsAsync();
            return Ok(seasons);
        }
        #endregion

        #region Teams
        [HttpGet("teams/{id}")]
        [ProducesResponseType(typeof(TeamModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TeamModel>> GetTeam(int id)
        {
            var team = await _bayTahminService.GetTeamByIdAsync(id);
            if (team == null)
                return NotFound();
            return Ok(team);
        }

        [HttpGet("teams/{id}/statistics")]
        [ProducesResponseType(typeof(TeamStatistics), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TeamStatistics>> GetTeamStatistics(int id, [FromQuery] int leagueId, [FromQuery] int season)
        {
            var stats = await _bayTahminService.GetTeamStatisticsAsync(id, leagueId, season);
            if (stats == null)
                return NotFound();
            return Ok(stats);
        }

        [HttpGet("teams/{id}/seasons")]
        [ProducesResponseType(typeof(IEnumerable<int>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<int>>> GetTeamSeasons(int id)
        {
            var seasons = await _bayTahminService.GetTeamSeasonsAsync(id);
            return Ok(seasons);
        }

        [HttpGet("teams/countries")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<string>>> GetTeamCountries()
        {
            var countries = await _bayTahminService.GetTeamCountriesAsync();
            return Ok(countries);
        }
        #endregion

        #region Venues
        [HttpGet("venues/{id}")]
        [ProducesResponseType(typeof(VenueModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VenueModel>> GetVenue(int id)
        {
            var venue = await _bayTahminService.GetVenueByIdAsync(id);
            if (venue == null)
                return NotFound();
            return Ok(venue);
        }

        [HttpGet("venues/search")]
        [ProducesResponseType(typeof(IEnumerable<VenueModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VenueModel>>> SearchVenues(
            [FromQuery] string name = null,
            [FromQuery] string city = null,
            [FromQuery] string country = null)
        {
            var venues = await _bayTahminService.GetVenuesBySearchAsync(name, city, country);
            return Ok(venues);
        }
        #endregion

        #region Standings
        [HttpGet("standings")]
        [ProducesResponseType(typeof(StandingsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StandingsResponse>> GetStandings([FromQuery] int leagueId, [FromQuery] int season)
        {
            var standings = await _bayTahminService.GetLeagueStandingsAsync(leagueId, season);
            if (standings == null)
                return NotFound();
            return Ok(standings);
        }
        #endregion

        #region Fixtures
        [HttpGet("fixtures/day")]
        [ProducesResponseType(typeof(IEnumerable<Fixture>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Fixture>>> GetLiveFixtures()
        {
            var fixtures = await _bayTahminService.GetLiveFixturesAsync();
            return Ok(fixtures);
        }

        [HttpGet("fixtures/headtohead")]
        [ProducesResponseType(typeof(IEnumerable<Fixture>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Fixture>>> GetHeadToHeadFixtures([FromQuery] string h2h)
        {
            var fixtures = await _bayTahminService.GetHeadToHeadFixturesAsync(h2h);
            return Ok(fixtures);
        }

        [HttpGet("fixtures/{id}")]
        [ProducesResponseType(typeof(Fixture), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Fixture>> GetMatchById(int id)
        {
            var fixture = await _bayTahminService.GetMatchByIdAsync(id);
            if (fixture == null)
                return NotFound();
            return Ok(fixture);
        }

        [HttpGet("fixtures/{id}/statistics")]
        [ProducesResponseType(typeof(IEnumerable<FixtureStatistics>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FixtureStatistics>>> GetFixtureStatistics(int id, [FromQuery] int? teamId = null)
        {
            var stats = await _bayTahminService.GetFixtureStatisticsAsync(id, teamId);
            return Ok(stats);
        }

        [HttpGet("fixtures/{id}/events")]
        [ProducesResponseType(typeof(IEnumerable<FixtureEvent>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FixtureEvent>>> GetFixtureEvents(int id)
        {
            var events = await _bayTahminService.GetFixtureEventsAsync(id);
            return Ok(events);
        }

        [HttpGet("fixtures/{id}/lineups")]
        [ProducesResponseType(typeof(IEnumerable<FixtureLineup>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FixtureLineup>>> GetFixtureLineups(int id)
        {
            var lineups = await _bayTahminService.GetFixtureLineupsAsync(id);
            return Ok(lineups);
        }

        [HttpGet("fixtures/{id}/players")]
        [ProducesResponseType(typeof(IEnumerable<FixturePlayer>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FixturePlayer>>> GetFixturePlayers(int id)
        {
            var players = await _bayTahminService.GetFixturePlayersAsync(id);
            return Ok(players);
        }
        #endregion

        #region Injuries
        [HttpGet("injuries/fixture/{id}")]
        [ProducesResponseType(typeof(IEnumerable<InjuryModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InjuryModel>>> GetFixtureInjuries(int id)
        {
            var injuries = await _bayTahminService.GetFixtureInjuriesAsync(id);
            return Ok(injuries);
        }

        [HttpGet("injuries/team/{id}")]
        [ProducesResponseType(typeof(IEnumerable<InjuryModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InjuryModel>>> GetTeamInjuries(int id, [FromQuery] int? leagueId = null, [FromQuery] int? season = null)
        {
            var injuries = await _bayTahminService.GetTeamInjuriesAsync(id, leagueId, season);
            return Ok(injuries);
        }

        [HttpGet("injuries/player/{id}")]
        [ProducesResponseType(typeof(IEnumerable<InjuryModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InjuryModel>>> GetPlayerInjuries(int id)
        {
            var injuries = await _bayTahminService.GetPlayerInjuriesAsync(id);
            return Ok(injuries);
        }
        #endregion

        #region Players
        [HttpGet("players/seasons")]
        [ProducesResponseType(typeof(IEnumerable<int>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<int>>> GetPlayerSeasons()
        {
            var seasons = await _bayTahminService.GetPlayerSeasonsAsync();
            return Ok(seasons);
        }

        [HttpGet("players/{id}/profile")]
        [ProducesResponseType(typeof(PlayerDetailedInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PlayerDetailedInfo>> GetPlayerProfile(int id)
        {
            var profile = await _bayTahminService.GetPlayerProfileAsync(id);
            if (profile == null)
                return NotFound();
            return Ok(profile);
        }

        [HttpGet("players/{id}/statistics")]
        [ProducesResponseType(typeof(IEnumerable<PlayerProfile>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PlayerProfile>>> GetPlayerStatistics(int id, [FromQuery] int season)
        {
            var stats = await _bayTahminService.GetPlayerStatisticsAsync(id, season);
            return Ok(stats);
        }

        [HttpGet("players/{id}/teams")]
        [ProducesResponseType(typeof(IEnumerable<PlayerTeam>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PlayerTeam>>> GetPlayerTeams(int id)
        {
            var teams = await _bayTahminService.GetPlayerTeamsAsync(id);
            return Ok(teams);
        }

        [HttpGet("players/topassists")]
        [ProducesResponseType(typeof(IEnumerable<TopPlayer>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TopPlayer>>> GetTopAssists([FromQuery] int leagueId, [FromQuery] int season)
        {
            var players = await _bayTahminService.GetTopAssistsAsync(leagueId, season);
            return Ok(players);
        }

        [HttpGet("players/topyellowcards")]
        [ProducesResponseType(typeof(IEnumerable<TopPlayer>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TopPlayer>>> GetTopYellowCards([FromQuery] int leagueId, [FromQuery] int season)
        {
            var players = await _bayTahminService.GetTopYellowCardsAsync(leagueId, season);
            return Ok(players);
        }

        [HttpGet("players/topredcards")]
        [ProducesResponseType(typeof(IEnumerable<TopPlayer>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TopPlayer>>> GetTopRedCards([FromQuery] int leagueId, [FromQuery] int season)
        {
            var players = await _bayTahminService.GetTopRedCardsAsync(leagueId, season);
            return Ok(players);
        }
        #endregion

        #region Transfers
        [HttpGet("transfers/player/{id}")]
        [ProducesResponseType(typeof(IEnumerable<TransferModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TransferModel>>> GetPlayerTransfers(int id)
        {
            var transfers = await _bayTahminService.GetPlayerTransfersAsync(id);
            return Ok(transfers);
        }
        #endregion

        #region Trophies
        [HttpGet("trophies/player/{id}")]
        [ProducesResponseType(typeof(IEnumerable<TrophyModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TrophyModel>>> GetPlayerTrophies(int id)
        {
            var trophies = await _bayTahminService.GetPlayerTrophiesAsync(id);
            return Ok(trophies);
        }
        #endregion

        #region Sidelined
        [HttpGet("sidelined/player/{id}")]
        [ProducesResponseType(typeof(IEnumerable<SidelinedModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SidelinedModel>>> GetPlayerSidelined(int id)
        {
            var sidelined = await _bayTahminService.GetPlayerSidelinedAsync(id);
            return Ok(sidelined);
        }
        #endregion

        #region Odds
        [HttpGet("odds/fixture/{id}")]
        [ProducesResponseType(typeof(IEnumerable<OddsModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OddsModel>>> GetFixtureOdds(
            int id,
            [FromQuery] int? bookmaker = null,
            [FromQuery] int? bet = null)
        {
            var odds = await _bayTahminService.GetFixtureOddsAsync(id, bookmaker, bet);
            return Ok(odds);
        }

        [HttpGet("odds/mapping")]
        [ProducesResponseType(typeof(IEnumerable<OddsMapping>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OddsMapping>>> GetOddsMapping()
        {
            var mapping = await _bayTahminService.GetOddsMappingAsync();
            return Ok(mapping);
        }

        [HttpGet("odds/bookmakers")]
        [ProducesResponseType(typeof(IEnumerable<BookmakerInfo>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookmakerInfo>>> GetBookmakers()
        {
            var bookmakers = await _bayTahminService.GetBookmakersAsync();
            return Ok(bookmakers);
        }

        [HttpGet("odds/bets")]
        [ProducesResponseType(typeof(IEnumerable<BetInfo>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BetInfo>>> GetBets()
        {
            var bets = await _bayTahminService.GetBetsAsync();
            return Ok(bets);
        }
        #endregion
    }
}
