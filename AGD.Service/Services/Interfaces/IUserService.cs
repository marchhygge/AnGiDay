using AGD.Service.DTOs.Request;
using AGD.Service.DTOs.Response;
using Microsoft.AspNetCore.Http;

namespace AGD.Service.Services.Interfaces
{
    public interface IUserService
    {
        Task<GoogleIdLoginResponse> LoginByGoogleWithIdTokenAsync(GoogleIdLoginRequest request, CancellationToken ct = default);
        Task<UserProfileResponse?> GetUserProfileAsync(int userId, CancellationToken ct = default);
        Task<UserProfileResponse?> UpdateUserProfileAsync(int userId, UpdateUserProfileRequest request, CancellationToken ct = default);
        Task<UserProfileResponse?> UpdateUserAvatarAsync(int userId, IFormFile avatarFile, CancellationToken ct = default);
        Task TriggerEmailVerificationAsync(int userId, CancellationToken ct = default);
        Task<bool> VerifyEmailAsync(string token, CancellationToken ct = default);
    }
}
