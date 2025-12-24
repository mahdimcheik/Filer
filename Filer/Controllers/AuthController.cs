using Filer.Models;
using Filer.Secvices;
using Microsoft.AspNetCore.Mvc;

namespace Filer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(AuthService authService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetToken(
            [FromQuery] string serviceName,
            [FromQuery] string[] scopes
        )
        {
            if (string.IsNullOrEmpty(serviceName) || scopes == null || scopes.Length == 0)
            {
                return BadRequest("Le nom du service et les scopes sont requis.");
            }
            try
            {
                var token = authService.GenerateToken(serviceName, scopes);
                return Ok(
                    new ResponseAuth
                    {
                        Token = token,
                        ExpiresIn = 31536000,
                    }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la génération du token : {ex.Message}");
            }
        }
    }
}
