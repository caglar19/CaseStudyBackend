using CaseStudy.Application.Models;
using CaseStudy.Application.Models.User;

namespace CaseStudy.Application.Services;

public interface IUserService
{
    Task<CreateUserResponseModel> CreateAsync(CreateUserModel createUserModel);

    Task<BaseResponseModel> ChangePasswordAsync(Guid refId, ChangePasswordModel changePasswordModel);

    Task<ConfirmEmailResponseModel> ConfirmEmailAsync(ConfirmEmailModel confirmEmailModel);

    Task<LoginResponseModel> LoginAsync(LoginUserModel loginUserModel);
}