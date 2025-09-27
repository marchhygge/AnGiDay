using AGD.Repositories.Models;
using AGD.Service.DTOs.Response;
using AGD.Service.Services.Implement;
using AGD.Service.Services.Interfaces;
using AGD.Service.Shared.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace AGD.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;
        public PostController(IServicesProvider servicesProvider)
        {
            _servicesProvider = servicesProvider;
        }

        [HttpGet("restaurant-post/{restaurantId:int}")]
        //[EnableQuery(PageSize = 10, MaxNodeCount = 80)]
        public async Task<ActionResult<IQueryable<Post>>> GetRestaurantPost([FromRoute] int restaurantId, CancellationToken ct = default)
        {
            var res = await _servicesProvider.RestaurantService.GetAsync(restaurantId, ct);

            if (res == null)
            {
                return BadRequest("Restaurant not exist");
            }
            return Ok(_servicesProvider.PostService.GetRestaurantPost(res.Id));
        }

        [HttpGet("feedback/{restaurantId:int}")]
        public async Task<ActionResult<ApiResult<IEnumerable<FeedbackResponse>>>> GetRestaurantFeedback([FromRoute] int restaurantId, CancellationToken ct = default)
        {
            var res = await _servicesProvider.RestaurantService.GetAsync(restaurantId, ct);

            if (res == null)
            {
                return ApiResult<IEnumerable<FeedbackResponse>>.FailResponse("Restaurant not exist");
            }

            var feedback = await _servicesProvider.PostService.GetRestaurantFeedback(restaurantId, ct);
            return ApiResult<IEnumerable<FeedbackResponse>>.SuccessResponse(feedback);
        }

        [HttpGet("detail-post/{postId}")]
        public async Task<ActionResult<ApiResult<DetailPostResponse>>> GetPostDetail([FromRoute] int postId, CancellationToken ct = default)
        {
            var result = await _servicesProvider.PostService.GetPostDetail(postId, ct);

            if (result == null)
                return ApiResult<DetailPostResponse>.FailResponse("Post not found");

            return ApiResult<DetailPostResponse>.SuccessResponse(result);
        }
    }
}
