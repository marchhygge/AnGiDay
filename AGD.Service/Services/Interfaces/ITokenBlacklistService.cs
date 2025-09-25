namespace AGD.Service.Services.Interfaces
{
    public interface ITokenBlacklistService
    {
        Task AddAsync(string jti, TimeSpan ttl);
        Task<bool> ExistsAsync(string jti);
    }
}
