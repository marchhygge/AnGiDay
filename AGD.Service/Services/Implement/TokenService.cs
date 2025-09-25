using AGD.Repositories.Helpers;
using AGD.Service.Services.Interfaces;

namespace AGD.Service.Services.Implement
{
    public class TokenService : ITokenService
    {
        private readonly JwtHelper _jwtHelper;
        public TokenService(JwtHelper jwtHelper)
        {
            _jwtHelper = jwtHelper;
        }
        public (string? Jti, DateTime? ExpiresUtc) ReadJtiAndExpiry(string token)
        {
            return _jwtHelper.ReadJtiAndExpiry(token);
        }
    }
}
