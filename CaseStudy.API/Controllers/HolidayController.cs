using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CaseStudy.Application.Models;
using CaseStudy.Application.Models.Holiday;
using CaseStudy.Application.Services;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CaseStudy.API.Controllers;

[Route("/core/api/[controller]/[action]")]
[ApiController]
[AllowAnonymous]
public class HolidayController : ControllerBase
{
    private readonly IHolidayService _holidayService;
    private readonly ILogger<HolidayController> _logger;

    // Yapıcı methodu doğru şekilde tanımladık.
    public HolidayController(IHolidayService holidayService, ILogger<HolidayController> logger)
    {
        _holidayService = holidayService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhookAsync([FromBody] ShopifyWebhookModel webhookData)
    {
        try
        {
            // Her isteğe her zaman 200 OK döndür
            _logger.LogInformation($"Received Webhook Data: {JsonSerializer.Serialize(webhookData)}");
            var response = new
            {
                grant_type = "password",
                username = "admin",
                password = "1q2w3E",
                client_id = "Hitframe_Entegration",
                client_secret = "1q2w3e*"
            };

            // 200 OK ile parametrel
            return Ok(response);
        }
        catch (Exception ex)
        {
            // Eğer bir hata oluşursa, 500 Internal Server Error döndür
            _logger.LogError($"Error processing webhook: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
    }



    private bool VerifyShopifySignature(HttpRequest request, string sharedSecret)
    {
        request.Headers.TryGetValue("X-Shopify-Hmac-Sha256", out var shopifySignature);

        // Body'yi okuyun
        using var reader = new StreamReader(request.Body);
        var body = reader.ReadToEnd();

        // Shared Secret ile imza hesaplayın
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sharedSecret));
        var computedSignature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));

        // Hesaplanan imza ile gelen imzayı karşılaştırın
        return computedSignature == shopifySignature;
    }

    [HttpPost]
    public async Task<IActionResult> GetCountryAsync()
    {
        var result = await _holidayService.GetCountryAsync();
        return Ok(ApiResult<List<CountryResponseModel>>.Success(result, result.Count));
    }

    [HttpGet]
    public async Task<IActionResult> GetSubdivisionAsync([FromQuery] SubdivisionRequestModel model)
    {
        var result = await _holidayService.GetSubdivisionAsync(model);
        return Ok(ApiResult<List<SubdivisionResponseModel>>.Success(result, result.Count));
    }

    [HttpGet]
    public async Task<IActionResult> GetHolidayAsync([FromQuery] HolidayRequestModel model)
    {
        List<HolidayResponseModel> result = new List<HolidayResponseModel>();

        if (model.HolidayType.Any(k => k == HolidayType.Public))
        {
            result.AddRange(await _holidayService.GetPublicHolidayAsync(model));
        }

        if (model.HolidayType.Any(k => k == HolidayType.School))
        {
            result.AddRange(await _holidayService.GetSchoolHolidayAsync(model));
        }

        result = result
            .GroupBy(h => new { h.StartDate, h.EndDate, h.Name })
            .Select(g => g.First())
            .ToList();

        return Ok(ApiResult<List<HolidayResponseModel>>.Success(result, result.Count));
    }
}
