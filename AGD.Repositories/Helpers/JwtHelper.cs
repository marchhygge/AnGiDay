using AGD.Repositories.ConfigurationModels;
using AGD.Repositories.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AGD.Repositories.Helpers
{
    public class JwtHelper
    {
        private readonly JwtSettings _settings;

        public JwtHelper(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public string GenerateToken(User user)
        {
            var now = DateTime.UtcNow;
            var jti  =  Guid.NewGuid().ToString("N");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.RoleId.ToString() ?? "user"),
                new Claim(JwtRegisteredClaimNames.Jti, jti),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                notBefore: now,
                claims: claims,
                expires: now.AddMinutes(_settings.ExpirationInMinutes),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            Console.WriteLine($"Generated JWT: {jwt}");
            return jwt;
        }

        public (string? Jti, DateTime? ExpiresUtc) ReadJtiAndExpiry(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                var exp = jwt.Payload.Expiration;
                DateTime? expUtc = exp.HasValue
                    ? DateTimeOffset.FromUnixTimeSeconds(exp.Value).UtcDateTime
                    : null;
                return (jti, expUtc);
            }
            catch
            {
                return (null, null);
            }
        }

        public string GenerateTokenForGoogleLogin(User user)
        {
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("username", user.Username),
            new("role", user.RoleId.ToString() ?? "user"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(_settings.ExpirationInMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
