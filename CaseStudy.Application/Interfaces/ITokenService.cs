using CaseStudy.Application.Models.Holiday;

namespace CaseStudy.Application.Interfaces;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(TokenRequest request);
}
