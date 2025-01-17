using CaseStudy.Application.Interfaces;
using CaseStudy.Application.Models.Holiday;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

namespace CaseStudy.Application.Services.Impl;

public class TokenService : ITokenService
{
    private readonly ILogger<TokenService> _logger;

    public TokenService(ILogger<TokenService> logger)
    {
        _logger = logger;
    }

    public async Task<string> GenerateTokenAsync(TokenRequest tokenRequest)
    {
        // Gelen veriyi kontrol et
        if (tokenRequest.GrantType != "password" ||
            tokenRequest.Username != "admin" ||
            tokenRequest.Password != "1q2w3E" ||
            tokenRequest.ClientId != "Hitframe_Entegration" ||
            tokenRequest.ClientSecret != "1q2w3e*")
        {
            throw new UnauthorizedAccessException("Geçersiz kullanıcı bilgileri.");
        }

        // Token oluşturmak için gerekli bilgileri ayarla
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, tokenRequest.Username),
            new Claim(ClaimTypes.Role, "admin"),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("supersecretkey1234567890123456789")); // 32 bytes (256 bits)
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "yourdomain.com",
            audience: "yourdomain.com",
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return jwtToken;
    }
}

