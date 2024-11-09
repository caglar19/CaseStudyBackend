namespace CaseStudy.Application.Models.User;

public class ConfirmEmailModel
{
    public string UserName { get; set; }

    public string Token { get; set; }
}

public class ConfirmEmailResponseModel
{
    public bool Confirmed { get; set; }
}