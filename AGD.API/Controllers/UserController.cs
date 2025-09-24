using AGD.Service.DTOs.Request;
using AGD.Service.DTOs.Response;
using AGD.Service.Services.Interfaces;
using AGD.Service.Shared.Result;
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
    }
}
