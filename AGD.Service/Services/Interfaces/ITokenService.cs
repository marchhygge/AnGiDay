namespace AGD.Service.Services.Interfaces
{
    public interface ITokenService
    {
        (string? Jti, DateTime? ExpiresUtc) ReadJtiAndExpiry(string token);
    }
}
