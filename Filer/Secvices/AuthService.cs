using Filer.Utilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Filer.Secvices;

public class AuthService
{
    public string GenerateToken(string serviceName)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(EnvironmentVariables.JwtSecret)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, serviceName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var token = new JwtSecurityToken(
            issuer: "storage-hub",
            audience: "internal-services",
            claims: claims,
            expires: DateTime.UtcNow.AddYears(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
