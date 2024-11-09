using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CaseStudy.Application.Models;
using CaseStudy.Application.Models.Holiday;
using CaseStudy.Application.Services;

namespace CaseStudy.API.Controllers;

[Route("/core/api/[controller]/[action]")]
[ApiController]
[Authorize]
public class HolidayController(IHolidayService holidayService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync(HolidayCreateModel createModel)
    {
        var result = await holidayService.CreateAsync(createModel);
        return Ok(ApiResult<HolidayCreateResponseModel>.Success(result, 1));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await holidayService.GetAllAsync();
        return Ok(ApiResult<List<HolidayResponseModel>>.Success(result, result.Count));
    }

    [HttpGet("{refId:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid refId)
    {
        return Ok(ApiResult<HolidayResponseModel>.Success(
            await holidayService.GetByIdAsync(refId), 1));
    }

    [HttpPut("{refId:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid refId, HolidayUpdateModel updateModel)
    {
        return Ok(ApiResult<HolidayUpdateResponseModel>.Success(
            await holidayService.UpdateAsync(refId, updateModel), 1));
    }

    [HttpDelete("{refId:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid refId)
    {
        return Ok(ApiResult<BaseResponseModel>.Success(await holidayService.DeleteAsync(refId), 1));
    }
}