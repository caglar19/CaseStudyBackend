namespace CaseStudy.Application.Models.User;

public class CreateUserModel
{
    public string Email { get; set; }

    public string Password { get; set; }
}

public class CreateUserResponseModel : BaseResponseModel
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public DateTime CreatedOn { get; set; }
}