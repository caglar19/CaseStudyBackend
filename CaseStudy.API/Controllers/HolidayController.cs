using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CaseStudy.Application.Models;
using CaseStudy.Application.Models.Holiday;
using CaseStudy.Application.Services;

namespace CaseStudy.API.Controllers;

[Route("/core/api/[controller]/[action]")]
[ApiController]
[AllowAnonymous]
public class HolidayController(IHolidayService holidayService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCountryAsync()
    {
        var result = await holidayService.GetCountryAsync();
        return Ok(ApiResult<List<CountryResponseModel>>.Success(result, result.Count));
    }

    [HttpGet]
    public async Task<IActionResult> GetSubdivisionAsync([FromQuery] SubdivisionRequestModel model)
    {
        var result = await holidayService.GetSubdivisionAsync(model);

        return Ok(ApiResult<List<SubdivisionResponseModel>>.Success(result, result.Count));
    }

    [HttpGet]
    public async Task<IActionResult> GetHolidayAsync([FromQuery] HolidayRequestModel model)
    {
        List<HolidayResponseModel> result = new List<HolidayResponseModel>();
        
        if (model.HolidayType.Any(k=> k == HolidayType.Public))
        {
            result.AddRange(await holidayService.GetPublicHolidayAsync(model));
        }

        if (model.HolidayType.Any(k => k == HolidayType.School))
        {
            result.AddRange(await holidayService.GetSchoolHolidayAsync(model));
        }
        
        return Ok(ApiResult<List<HolidayResponseModel>>.Success(result, result.Count));
    }
}