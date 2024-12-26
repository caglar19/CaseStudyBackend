using CaseStudy.Application.Models.Holiday;

namespace CaseStudy.Application.Services;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(TokenRequest request);
}
