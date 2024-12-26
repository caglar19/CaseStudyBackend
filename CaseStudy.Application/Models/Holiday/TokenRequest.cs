
namespace CaseStudy.Application.Models.Holiday
{
    public class TokenRequest
    {
        public string GrantType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

}
