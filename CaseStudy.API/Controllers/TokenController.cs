using CaseStudy.Application.Interfaces;
using CaseStudy.Application.Models.Holiday;
using Microsoft.AspNetCore.Mvc;

namespace CaseStudy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public TokenController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        // POST api/token/generate
        [HttpPost]
        public async Task<IActionResult> GenerateToken([FromBody] TokenRequest request)
        {
            try
            {
                // TokenService'i kullanarak token'ı oluştur
                var accessToken = await _tokenService.GenerateTokenAsync(request);

                // Token başarıyla oluşturulduğunda dön
                return Ok(new { AccessToken = accessToken });
            }
            catch (UnauthorizedAccessException ex)
            {
                // Hatalı kullanıcı bilgisi
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                // Genel hata durumu
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}