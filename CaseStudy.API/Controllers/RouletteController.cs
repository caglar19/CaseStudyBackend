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
        /// İlk rulet sayılarını yükler
        /// </summary>
        /// <param name="request">İlk yüklenecek rulet sayıları</param>
        /// <returns>Yükleme sonucu</returns>
        [HttpPost("initialize")]
        public async Task<ActionResult<RouletteInitializeResponse>> InitializeWithNumbers([FromBody] RouletteInitializeRequest request)
        {
            try
            {
                if (request == null || request.InitialNumbers == null || request.InitialNumbers.Count == 0)
                {
                    return BadRequest("Geçerli rulet sayıları sağlanmalıdır.");
                }

                var response = await _rouletteService.InitializeWithNumbers(request.InitialNumbers);
                if (!response.Success)
                {
                    return BadRequest("Rulet verileri yüklenemedi.");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rulet verileri yüklenirken hata oluştu");
                return StatusCode(500, "Rulet verileri yüklenirken bir hata oluştu.");
            }
        }

        /// <summary>
        /// Yeni bir rulet sayısı ekler ve bir sonraki sayıyı tahmin eder
        /// </summary>
        /// <param name="request">Yeni rulet sayısı</param>
        /// <returns>Tahmin sonucu</returns>
        [HttpPost("predict")]
        public async Task<ActionResult<RoulettePredictionResponse>> AddNumberAndPredict([FromBody] RouletteAddNumberRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Geçerli bir istek sağlanmalıdır.");
                }

                var response = await _rouletteService.AddNumberAndPredict(request.NewNumber);
                
                if (response.PredictedNumber < 0)
                {
                    return BadRequest("Tahmin yapılamadı. Lütfen önce rulet verilerini yükleyin.");
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
