using AGD.Repositories.Models;
using AGD.Service.DTOs.Request;
using AGD.Service.DTOs.Response;
using AGD.Service.Services.Implement;
using AGD.Service.Services.Interfaces;
using AGD.Service.Shared.Result;
using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AGD.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserTagController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;
        public UserTagController(IServicesProvider servicesProvider)
        {
            _servicesProvider = servicesProvider;
        }

        [HttpGet("{userId:int}")]
        public async Task<ActionResult<ApiResult<IEnumerable<UserTagResponse>>>> GetTagOfUser([FromRoute] int userId, CancellationToken ct)
        {
            var tags = await _servicesProvider.UserTagService.GetTagsOfUserAsync(userId, ct);

            if(tags != null && tags.Any())
            {
                return ApiResult<IEnumerable<UserTagResponse>>.SuccessResponse(tags);
            }
            return ApiResult<IEnumerable<UserTagResponse>>.FailResponse("User does not have tags");
        }

        [HttpPost("choose-tag")]
        public async Task<ActionResult<ApiResult<IEnumerable<UserTagResponse>>>> AddUpdateTagsAsync([FromBody] IEnumerable<UserTagRequest> request, CancellationToken ct)
        {
            try
            {
                var results = await _servicesProvider.UserTagService.AddUpdateUserTag(request, ct);

                if (results.All(r => r.IsDeleted == true))
                {
                    return ApiResult<IEnumerable<UserTagResponse>>
                        .SuccessResponse(results, "Remove tags successfully");
                }

                return ApiResult<IEnumerable<UserTagResponse>>
                    .SuccessResponse(results, "Choose tags successfully", 201);
            }
            catch (Exception ex)
            {
                return ApiResult<IEnumerable<UserTagResponse>>
                    .FailResponse($"Choose failed: {ex.Message}", 400);
            }
        }
    }
}
