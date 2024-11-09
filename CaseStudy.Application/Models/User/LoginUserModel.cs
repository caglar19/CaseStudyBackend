namespace CaseStudy.Application.Models.User;

public class LoginUserModel
{
    public string Email { get; set; }

    public string Password { get; set; }
}

public class LoginResponseModel
{
    public string Token { get; set; }
}