using AGD.Service.DTOs.Response;
using AGD.Service.Services.Interfaces;
using AGD.Service.Shared.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AGD.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;
        public TagController(IServicesProvider servicesProvider)
        {
            _servicesProvider = servicesProvider;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResult<IEnumerable<TagResponse>>>> GetAllTags (CancellationToken ct)
        {
            var tags = await _servicesProvider.TagService.GetTags(ct);

            if(tags != null)
            {
                return ApiResult<IEnumerable<TagResponse>>.SuccessResponse(tags);
            }
            return ApiResult<IEnumerable<TagResponse>>.FailResponse("Tag not found");           
        }
    }
}
