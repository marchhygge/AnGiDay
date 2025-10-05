using AGD.Repositories.Models;
using AGD.Service.DTOs.Request;
using AGD.Service.DTOs.Response;
using AGD.Service.Services.Implement;
using AGD.Service.Services.Interfaces;
using AGD.Service.Shared.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Formats.Asn1;

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

        [HttpPost("like")]
        public async Task<ActionResult<ApiResult<LikeResponse>>> LikePost([FromBody] LikeRequest request, CancellationToken ct = default)
        {
            try
            {
                var result = await _servicesProvider.PostService.AddLikeAsync(request, ct);
               
                if (result.IsDeleted)
                    return ApiResult<LikeResponse>.SuccessResponse(result, "Unlike successfully", 201);

                return ApiResult<LikeResponse>.SuccessResponse(result, "Like successfully", 201);
            }
            catch (Exception ex)
            {
                return ApiResult<LikeResponse>.FailResponse($"Like failed: {ex.Message}", 400);
            }
        }

        [HttpPost("create-post")]
        public async Task<ActionResult<ApiResult<PostResponse>>> CreatePost ([FromBody] PostRequest request, CancellationToken ct = default)
        {
            try
            {
                var result = await _servicesProvider.PostService.CreatePostAsync(request, ct);

                return ApiResult<PostResponse>.SuccessResponse(result, "Create post successfully", 201);
            }
            catch (Exception ex)
            {
                return ApiResult<PostResponse>.FailResponse($"Create post fail: {ex}", 400);
            }
        }

        [HttpGet("type/{type}")]
        public async Task<ActionResult<ApiResult<IEnumerable<PostResponse>>>> GetPostsByType(string type, CancellationToken ct)
        {
            var posts = await _servicesProvider.PostService.GetPostsByTypeAsync(type, ct);
            if(posts == null)
                return ApiResult<IEnumerable<PostResponse>>.FailResponse($"Post with {type} not found");

            return ApiResult<IEnumerable<PostResponse>>.SuccessResponse(posts);
        }

        [HttpPut("{postId:int}")]
        public async Task<ActionResult<ApiResult<PostResponse>>> UpdatePost ([FromRoute]int postId, [FromBody] PostRequest request, CancellationToken ct)
        {
            try
            {
                var updated = await _servicesProvider.PostService.UpdatePostAsync(postId, request, ct);

                if (updated == null)
                    return ApiResult<PostResponse>.FailResponse("Post not found", 404);
                
                return ApiResult<PostResponse>.SuccessResponse(updated, "Update successful");
            }
            catch (Exception ex)
            {
                return ApiResult<PostResponse>.FailResponse($"Update failed: {ex.Message}", 400);
            }
        }

        [HttpDelete("delete/{postId:int}")]
        public async Task<ActionResult<ApiResult<string>>> DeletePost(int postId, CancellationToken ct)
        {
            var success = await _servicesProvider.PostService.DeletePostAsync(postId, ct);
            if (!success) return ApiResult<string>.FailResponse("Post not found", 404);

            return ApiResult<string>.SuccessResponse("Post deleted successfully");
        }
    }
}
