using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using CaseStudy.Core.Entities;

namespace CaseStudy.Application.Helpers;

public static class JwtHelper
{
    public static string GenerateToken(User user, IConfiguration configuration)
    {
        var secretKey = configuration.GetValue<string>("JwtConfiguration:SecretKey");

        var key = Encoding.ASCII.GetBytes(secretKey??string.Empty);

        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.RefId.ToString()),
                new Claim(ClaimTypes.Name, user.RefId.ToString()),
                new Claim(ClaimTypes.Email, user.RefId.ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}