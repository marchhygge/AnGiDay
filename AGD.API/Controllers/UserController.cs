using AGD.Service.DTOs.Request;
using AGD.Service.DTOs.Response;
using AGD.Service.Services.Interfaces;
using AGD.Service.Shared.Result;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace AGD.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;
        public UserController(IServicesProvider servicesProvider)
        {
            _servicesProvider = servicesProvider;
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
                return ApiResult<GoogleIdLoginResponse>.FailResponse($"Invalid Google ID Token: {ex.Message}", 401);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                return ApiResult<GoogleIdLoginResponse>.FailResponse("Google ID Token configuration error.", 500);
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

        [HttpPost("register")]
        public async Task<ActionResult<ApiResult<RegisterUserResponse>>> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
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
    }
}
