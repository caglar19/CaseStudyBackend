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

        public BayTahminController(IBayTahminService bayTahminService)
        {
            _bayTahminService = bayTahminService;
        }

        [HttpGet("leagues")]
        public async Task<ActionResult<IEnumerable<LeagueModel>>> GetLeagues(string country = null)
        {
            var leagues = await _bayTahminService.GetLeaguesAsync(country);
            return Ok(leagues);
        }

        [HttpGet("leagues/{id}")]
        public async Task<ActionResult<LeagueModel>> GetLeague(int id)
        {
            var league = await _bayTahminService.GetLeagueByIdAsync(id);
            if (league == null)
                return NotFound();
            return Ok(league);
        }

        [HttpGet("teams/league/{leagueId}")]
        public async Task<ActionResult<IEnumerable<TeamModel>>> GetTeamsByLeague(int leagueId)
        {
            var teams = await _bayTahminService.GetTeamsByLeagueAsync(leagueId);
            return Ok(teams);
        }

        [HttpGet("matches/league/{leagueId}")]
        public async Task<ActionResult<IEnumerable<MatchModel>>> GetMatchesByLeague(int leagueId)
        {
            var matches = await _bayTahminService.GetMatchesByLeagueAsync(leagueId);
            return Ok(matches);
        }

        [HttpGet("matches/upcoming/{leagueId}")]
        public async Task<ActionResult<IEnumerable<MatchModel>>> GetUpcomingMatches(int leagueId)
        {
            var matches = await _bayTahminService.GetUpcomingMatchesAsync(leagueId);
            return Ok(matches);
        }

        [HttpGet("matches/{id}/statistics")]
        public async Task<ActionResult<MatchStatisticsModel>> GetMatchStatistics(int id)
        {
            var statistics = await _bayTahminService.GetMatchStatisticsAsync(id);
            if (statistics == null)
                return NotFound();
            return Ok(statistics);
        }

        [HttpGet("matches/{id}/odds")]
        public async Task<ActionResult<IEnumerable<OddsModel>>> GetMatchOdds(int id)
        {
            var odds = await _bayTahminService.GetOddsByMatchAsync(id);
            return Ok(odds);
        }

        [HttpGet("players/team/{teamId}")]
        public async Task<ActionResult<IEnumerable<PlayerModel>>> GetPlayersByTeam(int teamId)
        {
            var players = await _bayTahminService.GetPlayersByTeamAsync(teamId);
            return Ok(players);
        }

        [HttpGet("predictions/match/{matchId}")]
        public async Task<ActionResult<IEnumerable<PredictionModel>>> GetMatchPredictions(int matchId)
        {
            var predictions = await _bayTahminService.GetPredictionsByMatchAsync(matchId);
            return Ok(predictions);
        }

        [HttpPost("predictions")]
        public async Task<ActionResult<PredictionModel>> CreatePrediction(PredictionModel prediction)
        {
            var createdPrediction = await _bayTahminService.CreatePredictionAsync(prediction);
            return CreatedAtAction(nameof(GetMatchPredictions), new { matchId = prediction.MatchId }, createdPrediction);
        }

        [HttpGet("transfers/player/{playerId}")]
        public async Task<ActionResult<IEnumerable<TransferModel>>> GetPlayerTransfers(int playerId)
        {
            var transfers = await _bayTahminService.GetTransfersByPlayerAsync(playerId);
            return Ok(transfers);
        }

        [HttpGet("injuries/team/{teamId}")]
        public async Task<ActionResult<IEnumerable<InjuryModel>>> GetTeamInjuries(int teamId)
        {
            var injuries = await _bayTahminService.GetActiveInjuriesByTeamAsync(teamId);
            return Ok(injuries);
        }

        // Sync endpoints for admin use
        [HttpPost("sync/league/{leagueId}/{season}")]
        public async Task<IActionResult> SyncLeagueData(int leagueId, int season)
        {
            await _bayTahminService.SyncLeagueDataAsync(leagueId, season);
            return Ok();
        }

        [HttpPost("sync/fixtures/{leagueId}/{season}")]
        public async Task<IActionResult> SyncFixtures(int leagueId, int season)
        {
            await _bayTahminService.SyncFixturesAsync(leagueId, season);
            return Ok();
        }

        [HttpPost("sync/live-scores/{leagueId}")]
        public async Task<IActionResult> SyncLiveScores(int leagueId)
        {
            await _bayTahminService.SyncLiveScoresAsync(leagueId);
            return Ok();
        }
    }
}
