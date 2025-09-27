using AGD.Service.DTOs.Request;
using AGD.Service.DTOs.Response;
using AGD.Service.Services.Interfaces;
using AGD.Service.Shared.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AGD.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;
        private readonly ILogger<UserController> _logger;
        public UserController(IServicesProvider servicesProvider, ILogger<UserController> logger)
        {
            _servicesProvider = servicesProvider;
            _logger = logger;
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(id, out userId);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var auth = Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(auth) || !auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "Missing bearer token" });

            var token = auth["Bearer ".Length..].Trim();
            var (jti, expUtc) = _servicesProvider.TokenService.ReadJtiAndExpiry(token);
            if (jti == null || expUtc == null)
                return BadRequest(new { error = "Invalid token" });

            var now = DateTime.UtcNow;
            var ttl = expUtc.Value - now;
            if (ttl <= TimeSpan.Zero)
                return Ok(new { message = "Token already expired" });

            await _servicesProvider.TokenBlacklistService.AddAsync(jti, ttl);
            return Ok(new { message = "Logged out" });
        }

        [HttpPost("login-by-google-id-token")]
        public async Task<ActionResult<ApiResult<GoogleIdLoginResponse>>> LoginByGoogle([FromBody] GoogleIdLoginRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.IdToken))
            {
                return ApiResult<GoogleIdLoginResponse>.FailResponse("Missing Id Token.");       
            }

            try
            {
                var res = await _servicesProvider.UserService.LoginByGoogleWithIdTokenAsync(request, ct);
                return ApiResult<GoogleIdLoginResponse>.SuccessResponse(res);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.Message);
                return ApiResult<GoogleIdLoginResponse>.FailResponse("Google ID Token Login unauthorized.", 401);
            }
            catch (Google.Apis.Auth.InvalidJwtException ex)
            {
                _logger.LogError(ex, "Google ID token configuration/runtime error.");
                return ApiResult<GoogleIdLoginResponse>.FailResponse($"Invalid Google ID Token: {ex.Message}", 401);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                return ApiResult<GoogleIdLoginResponse>.FailResponse("Google ID Token configuration error.", 500);
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResult<UserProfileResponse?>>> GetMyProfile(CancellationToken ct = default)
        {
            if (!TryGetUserId(out int userId))
            {
                return ApiResult<UserProfileResponse?>.FailResponse("Unauthorized", 401);
            }

            var profile = await _servicesProvider.UserService.GetUserProfileAsync(userId, ct);
            if (profile == null)
                return ApiResult<UserProfileResponse?>.FailResponse("User not found", 404);

            return ApiResult<UserProfileResponse?>.SuccessResponse(profile);
        }

        [HttpPut("me")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResult<UserProfileResponse?>>> UpdateMyProfile([FromForm] UpdateUserProfileRequest? request, CancellationToken ct = default)
        {
            if (!TryGetUserId(out int userId))
                return ApiResult<UserProfileResponse?>.FailResponse("Unauthorized", 401);

            if (request is null)
                return ApiResult<UserProfileResponse?>.FailResponse("Request body is required.", 400);

            if (request.DateOfBirth is { } dob && dob > DateOnly.FromDateTime(DateTime.UtcNow))
                ModelState.AddModelError(nameof(request.DateOfBirth), "Date of birth cannot be in the future.");

            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values.SelectMany(v => v.Errors)
                                                  .FirstOrDefault()?.ErrorMessage ?? "Invalid request data";
                return ApiResult<UserProfileResponse?>.FailResponse(firstError, 400);
            }
            try
            {
                var updated = await _servicesProvider.UserService.UpdateUserProfileAsync(userId, request, ct);
                if (updated == null)
                    return ApiResult<UserProfileResponse?>.FailResponse("User not found", 404);

                var baseMsg = updated.IsProfileComplete ? "Updated" : "Updated (incomplete profile)";
                if (!updated.IsEmailVerified)
                    baseMsg += " - email verification pending";
                return ApiResult<UserProfileResponse?>.SuccessResponse(updated, baseMsg);
            }
            catch (InvalidOperationException ex)
            {
                return ApiResult<UserProfileResponse?>.FailResponse(ex.Message, 409);
            }
        }

        [HttpPost("login-with-username")]
        public async Task<ActionResult<ApiResult<LoginUserNameResponse>>> LoginWithUsername([FromBody] LoginUserNameRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.username) && string.IsNullOrWhiteSpace(request.password))
            {
                return ApiResult<LoginUserNameResponse>.FailResponse("Missing valid username and pasword");
            }

            if (string.IsNullOrWhiteSpace(request.username))
            {
                return ApiResult<LoginUserNameResponse>.FailResponse("Missing username");
            }

            if (string.IsNullOrWhiteSpace(request.password))
            {
                return ApiResult<LoginUserNameResponse>.FailResponse("Missing password");
            }
            try 
            {
                var user = await _servicesProvider.UserService.LoginWithUsernameAsync(request, ct);
                return ApiResult<LoginUserNameResponse>.SuccessResponse(user, "Login success", 201);
            }
            catch(UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.Message);
                return ApiResult<LoginUserNameResponse>.FailResponse("Token Login unauthorized.", 401);
            }
        }

        [HttpPut("me/avatar")]
        [Authorize]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5_242_880)] // ~5MB
        public async Task<ActionResult<ApiResult<UserProfileResponse?>>> UpdateMyAvatar([FromForm] UpdateAvatarRequest request, CancellationToken ct = default)
        {
            if (!TryGetUserId(out int userId))
                return ApiResult<UserProfileResponse?>.FailResponse("Unauthorized", 401);

            if (request.Avatar is null)
                return ApiResult<UserProfileResponse?>.FailResponse("Avatar file is required.", 400);

            try
            {
                var updated = await _servicesProvider.UserService.UpdateUserAvatarAsync(userId, request.Avatar, ct);
                if (updated == null)
                    return ApiResult<UserProfileResponse?>.FailResponse("User not found", 404);

                return ApiResult<UserProfileResponse?>.SuccessResponse(updated, "Avatar updated");
            }
            catch (InvalidOperationException ex)
            {
                return ApiResult<UserProfileResponse?>.FailResponse(ex.Message, 400);
            }
        }

        [HttpPost("verify-email/send")]
        [Authorize]
        public async Task<ActionResult<ApiResult<string>>> SendVerifyEmail(CancellationToken ct = default)
        {
            if (!TryGetUserId(out var userId))
                return ApiResult<string>.FailResponse("Unauthorized", 401);

            await _servicesProvider.UserService.TriggerEmailVerificationAsync(userId, ct);
            return ApiResult<string>.SuccessResponse("Đã gửi email xác minh (nếu email chưa được xác minh).");
        }

        [HttpGet("verify-email")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResult<string>>> VerifyEmailLink([FromQuery] string token, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(token))
                return ApiResult<string>.FailResponse("Token không hợp lệ.", 400);

            var ok = await _servicesProvider.UserService.VerifyEmailAsync(token, ct);
            if (!ok) return ApiResult<string>.FailResponse("Xác minh thất bại hoặc token hết hạn.", 400);

            return ApiResult<string>.SuccessResponse("Xác minh email thành công.");
        }

        [HttpPost("register")]
        public async Task<ApiResult<RegisterUserResponse>> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
        {
            try
            {
                var res = await _servicesProvider.UserService.RegisterUserAsync(request, ct);
                return ApiResult<RegisterUserResponse>.SuccessResponse(res, "Register successfully", 201);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ApiResult<RegisterUserResponse>.FailResponse("Register fail");
            }
        }

        [HttpGet("community/post")]
        [AllowAnonymous]
        public async Task<ApiResult<IEnumerable<CommunityPostResponse>>> GetCommunityPost(CancellationToken ct)
        {
            var post = await _servicesProvider.UserService.GetCommunityPost(ct);

            if(post == null)
            {
                return ApiResult<IEnumerable<CommunityPostResponse>>.FailResponse("Post in community is empty");
            }

            return ApiResult<IEnumerable<CommunityPostResponse>>.SuccessResponse(post);
        }
    }
}
