using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CaseStudy.Application.Models;
using CaseStudy.Application.Models.Holiday;
using CaseStudy.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CaseStudy.API.Controllers
{
  [Route("core/api/[controller]/[action]")]
[ApiController]
[AllowAnonymous]
public class HolidayController : ControllerBase
{
    private readonly IHolidayService _holidayService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<HolidayController> _logger;

    // Shopify webhook allows requests only from this domain
    private readonly string _allowedShopDomain = "caglarcompany.myshopify.com";

    public HolidayController(
        IHolidayService holidayService,
        ITokenService tokenService,
        ILogger<HolidayController> logger)
    {
        _holidayService = holidayService;
        _tokenService = tokenService;
        _logger = logger;
    }

    #region Webhook Handling
    // Handle incoming Shopify webhook requests
    [HttpPost]
    public async Task<IActionResult> HandleWebhookAsync([FromBody] ShopifyWebhookModel webhookData)
    {
        try
        {
            // Extract the Shopify domain from the request headers
            var shopDomain = Request.Headers["X-Shopify-Shop-Domain"].ToString();

            // Validate the domain against the allowed one
            if (shopDomain != _allowedShopDomain)
            {
                return StatusCode(403, "Forbidden: Invalid Shopify shop domain");
            }

            // Prepare the OAuth2 parameters for token generation
            var tokenRequest = new TokenRequest
            {
                GrantType = "password",
                Username = "admin",
                Password = "1q2w3E",
                ClientId = "Hitframe_Entegration",
                ClientSecret = "1q2w3e*"
            };

            // Use the TokenService to generate the token
            var accessToken = await _tokenService.GenerateTokenAsync(tokenRequest);

            // If the token is missing, return an error
            if (string.IsNullOrEmpty(accessToken))
            {
                return StatusCode(500, "Failed to retrieve access token");
            }

            // Successfully retrieved the token
            return Ok(new { Message = "Webhook handled successfully", Token = accessToken });
        }
        catch (Exception ex)
        {
            // Log and return a server error if an exception occurs
            _logger.LogError(ex, "Error handling webhook");
            return StatusCode(500, "Internal Server Error");
        }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; }
    }
    #endregion

        #region Actions - Holiday Management

        // Retrieve all available countries for holidays
        [HttpPost]
        public async Task<IActionResult> GetCountryAsync()
        {
            try
            {
                var result = await _holidayService.GetCountryAsync();
                return Ok(ApiResult<List<CountryResponseModel>>.Success(result, result.Count));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving countries");
                return StatusCode(500, "Error retrieving countries");
            }
        }

        // Get subdivisions (regions) based on the request model
        [HttpGet]
        public async Task<IActionResult> GetSubdivisionAsync([FromQuery] SubdivisionRequestModel model)
        {
            try
            {
                var result = await _holidayService.GetSubdivisionAsync(model);
                return Ok(ApiResult<List<SubdivisionResponseModel>>.Success(result, result.Count));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subdivisions");
                return StatusCode(500, "Error retrieving subdivisions");
            }
        }

        // Get holidays based on the provided holiday request
        [HttpGet]
        public async Task<IActionResult> GetHolidayAsync([FromQuery] HolidayRequestModel model)
        {
            try
            {
                var result = new List<HolidayResponseModel>();

                // Add public holidays if requested
                if (model.HolidayType.Contains(HolidayType.Public))
                {
                    result.AddRange(await _holidayService.GetPublicHolidayAsync(model));
                }

                // Add school holidays if requested
                if (model.HolidayType.Contains(HolidayType.School))
                {
                    result.AddRange(await _holidayService.GetSchoolHolidayAsync(model));
                }

                // Remove duplicate holidays
                result = result
                    .GroupBy(h => new { h.StartDate, h.EndDate, h.Name })
                    .Select(g => g.First())
                    .ToList();

                return Ok(ApiResult<List<HolidayResponseModel>>.Success(result, result.Count));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving holidays");
                return StatusCode(500, "Error retrieving holidays");
            }
        }
        #endregion
    }
}
