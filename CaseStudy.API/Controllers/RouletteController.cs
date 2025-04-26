using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using CaseStudy.Application.Interfaces;
using CaseStudy.Application.Models.Roulette;
using System.Text.Json;

// Kullanılacak modelleri açıkça belirt
using RoulettePredictionResponseModel = CaseStudy.Application.Models.Roulette.RoulettePredictionResponse;
using RouletteInitializeResponseModel = CaseStudy.Application.Models.Roulette.RouletteInitializeResponse;
using RouletteExtractNumbersResponseModel = CaseStudy.Application.Models.Roulette.RouletteExtractNumbersResponse;
using RouletteInitializeRequestModel = CaseStudy.Application.Models.Roulette.RouletteInitializeRequest;
using RouletteAddNumberRequestModel = CaseStudy.Application.Models.Roulette.RouletteAddNumberRequest;
using RouletteExtractNumbersRequestModel = CaseStudy.Application.Models.Roulette.RouletteExtractNumbersRequest;

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
        public async Task<ActionResult<RouletteInitializeResponseModel>> InitializeWithNumbers([FromBody] RouletteInitializeRequestModel request)
        {
            try
            {
                if (request == null || request.InitialNumbers == null || request.InitialNumbers.Count == 0)
                {
                    return BadRequest("Geçerli rulet sayıları sağlanmalıdır.");
                }

                var response = await _rouletteService.InitializeNumbersAsync(request.InitialNumbers);
                if (response == null)
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
        /// <param name="newNumber">Yeni rulet sayısı</param>
        /// <returns>Tahmin sonucu</returns>
        [HttpPost("predict")]
        public async Task<IActionResult> AddNumberAndPredict([FromBody] int newNumber)
        {
            _logger.LogInformation($"AddNumberAndPredict endpoint'i çağrıldı. Yeni sayı: {newNumber}");
            var result = await _rouletteService.AddNumberAndPredict(newNumber);
            return Ok(result);
        }
        
        /// <summary>
        /// HTML içeriğinden rulet sayılarını çıkarır (Form veri olarak)
        /// </summary>
        /// <returns>Çıkarılan rulet sayıları</returns>
        [HttpPost("extract-numbers-form")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ExtractNumbersFromHtmlForm(IFormFile htmlFile)
        {
            try
            {
                _logger.LogInformation("ExtractNumbersFromHtmlForm endpoint'i çağrıldı.");
                
                if (htmlFile == null || htmlFile.Length == 0)
                {
                    _logger.LogWarning("Geçersiz istek: HTML dosyası boş veya null");
                    return BadRequest(new RouletteExtractNumbersResponse
                    {
                        Success = false,
                        ErrorMessage = "HTML dosyası boş veya geçersiz"
                    });
                }
                
                string htmlContent;
                using (var reader = new StreamReader(htmlFile.OpenReadStream()))
                {
                    htmlContent = await reader.ReadToEndAsync();
                }
                
                if (string.IsNullOrEmpty(htmlContent))
                {
                    return BadRequest(new RouletteExtractNumbersResponse
                    {
                        Success = false,
                        ErrorMessage = "HTML içeriği boş"
                    });
                }
                
                var result = await _rouletteService.ExtractNumbersFromHtml(htmlContent);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTML dosyasından rulet sayıları çıkarılırken hata oluştu");
                return StatusCode(500, "HTML dosyasından rulet sayıları çıkarılırken bir hata oluştu.");
            }
        }
    }
}
