using AGD.Repositories.ConfigurationModels;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Repositories.Helpers
{
    public class JwtHelper
    {
        private readonly JwtSettings _settings;

        public JwtHelper(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        //public string GenerateToken(User user)
        //{
        //    var claims = new[]
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        //        new Claim(ClaimTypes.Email, user.Email),
        //        new Claim(ClaimTypes.Name, user.FullName),
        //        new Claim(ClaimTypes.Role, user.Role),
        //    };

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    var token = new JwtSecurityToken(
        //        issuer: _settings.Issuer,
        //        audience: _settings.Audience,
        //        claims: claims,
        //        expires: DateTime.Now.AddMinutes(_settings.ExpirationInMinutes),
        //        signingCredentials: creds
        //    );

        //    var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        //    Console.WriteLine($"Generated JWT: {jwt}");
        //    return jwt;
        //}
    }
}
