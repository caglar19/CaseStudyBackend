using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CaseStudy.Application.Models;
using CaseStudy.Application.Models.User;
using CaseStudy.Application.Services;

namespace CaseStudy.API.Controllers;

[Route("/core/api/[controller]/[action]")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterAsync(CreateUserModel createUserModel)
    {
        return Ok(ApiResult<CreateUserResponseModel>.Success(await userService.CreateAsync(createUserModel), 1));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync(LoginUserModel loginUserModel)
    {
        return Ok(ApiResult<LoginResponseModel>.Success(await userService.LoginAsync(loginUserModel), 1));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmailAsync(ConfirmEmailModel confirmEmailModel)
    {
        return Ok(ApiResult<ConfirmEmailResponseModel>.Success(
            await userService.ConfirmEmailAsync(confirmEmailModel), 1));
    }

    [HttpPut("{refId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> ChangePassword(Guid refId, ChangePasswordModel changePasswordModel)
    {
        return Ok(ApiResult<BaseResponseModel>.Success(
            await userService.ChangePasswordAsync(refId, changePasswordModel), 1));
    }
}