using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CaseStudy.Application.Models;
using CaseStudy.Application.Models.Holiday;
using CaseStudy.Application.Services;
using CaseStudy.Application.Services.Impl;
using static CaseStudy.Application.Services.Impl.HolidayService;

namespace CaseStudy.API.Controllers;

[Route("/core/api/[controller]/[action]")]
[ApiController]
[AllowAnonymous]
public class HolidayController(IHolidayService holidayService) : ControllerBase
{
    [HttpGet("countries")]
    public async Task<IActionResult> GetCountryCodesAsync()
    {
        var result = await holidayService.GetCountryCodesAsync();
        return Ok(ApiResult<List<string>>.Success(result, result.Count));
    }

    [HttpGet("languages")]
    public async Task<IActionResult> GetLanguagesAsync()
    {
        var result = await holidayService.GetLanguagesAsync();

        return Ok(ApiResult<List<LanguageWithCode>>.Success(result, result.Count));
    }

    [HttpGet("subdivisions")]
    public async Task<IActionResult> GetSubdivisionsAsync([FromQuery] string countryIsoCode, [FromQuery] string languageIsoCode)
    {
        var result = await holidayService.GetSubdivisionsAsync(countryIsoCode, languageIsoCode);

        return Ok(ApiResult<List<Subdivision>>.Success(result, result.Count));
    }

    [HttpGet("public-holidays")]
    public async Task<IActionResult> GetPublicHolidaysAsync(
    [FromQuery] string countryIsoCode,
    [FromQuery] string languageIsoCode,
    [FromQuery] DateTime validFrom,
    [FromQuery] DateTime validTo,
    [FromQuery] string subdivisionCode)
    {
        var result = await holidayService.GetPublicHolidaysAsync(countryIsoCode, languageIsoCode, validFrom, validTo, subdivisionCode);
        return Ok(ApiResult<List<HolidayResponseModel>>.Success(result, result.Count));
    }
    [HttpGet("school-holidays")]
    public async Task<IActionResult> GetSchoolHolidaysAsync(
    [FromQuery] string countryIsoCode,
    [FromQuery] string languageIsoCode,
    [FromQuery] DateTime validFrom,
    [FromQuery] DateTime validTo,
    [FromQuery] string subdivisionCode)
    {
        var result = await holidayService.GetSchoolHolidaysAsync(countryIsoCode, languageIsoCode, validFrom, validTo, subdivisionCode);
        return Ok(ApiResult<List<SchoolHolidayResponseModel>>.Success(result, result.Count));
    }

}