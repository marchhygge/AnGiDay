using AGD.Repositories.Models;
using AGD.Service.DTOs.Request;
using AGD.Service.DTOs.Response;

namespace AGD.Service.Services.Interfaces
{
    public interface IUserService
    {
        Task<GoogleIdLoginResponse> LoginByGoogleWithIdTokenAsync(GoogleIdLoginRequest request, CancellationToken ct = default);
        Task<LoginUserNameResponse> LoginWithUsernameAsync(LoginUserNameRequest request, CancellationToken ct = default);
        Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request, CancellationToken ct = default);
    }
}
