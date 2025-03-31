using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CaseStudy.Application.Interfaces;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.API.Controllers
{
    [ApiController]
    [Route("api/roulette")]
    public class RouletteController : ControllerBase
    {
        private readonly IRouletteService _rouletteService;
        private readonly ILogger<RouletteController> _logger;

        public RouletteController(IRouletteService rouletteService, ILogger<RouletteController> logger)
        {
            _rouletteService = rouletteService;
            _logger = logger;
        }

        /// <summary>
        /// Rulet tahmin API'si
        /// İlk çağrıda initialNumbers ile 500 sayıyı yükler, sonraki çağrılarda newNumber ile tahmin yapar
        /// </summary>
        /// <param name="request">Tahmin isteği</param>
        /// <returns>Tahmin sonucu</returns>
        [HttpPost]
        public async Task<ActionResult<RoulettePredictionResponse>> PredictRoulette([FromBody] RoulettePredictionRequest request)
        {
            try
            {
                if (request == null || (request.InitialNumbers == null || request.InitialNumbers.Count == 0) && !request.NewNumber.HasValue)
                {
                    return BadRequest("Geçerli bir istek sağlanmalıdır: initialNumbers veya newNumber gereklidir.");
                }

                var response = await _rouletteService.PredictRoulette(request.InitialNumbers, request.NewNumber);
                
                if (response.PredictedNumber < 0)
                {
                    return BadRequest("Tahmin yapılamadı. Lütfen önce initialNumbers ile rulet verilerini yükleyin.");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rulet tahmini sırasında hata oluştu");
                return StatusCode(500, "Rulet tahmini yapılırken bir hata oluştu.");
            }
        }
    }
}
