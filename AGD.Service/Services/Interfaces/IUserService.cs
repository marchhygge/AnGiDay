using AGD.Service.DTOs.Request;
using AGD.Service.DTOs.Response;

namespace AGD.Service.Services.Interfaces
{
    public interface IUserService
    {
        Task<GoogleIdLoginResponse> LoginByGoogleWithIdTokenAsync(GoogleIdLoginRequest request, CancellationToken ct = default);
    }
}
