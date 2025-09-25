using AGD.Repositories.Models;
using AGD.Service.Services.Interfaces;
using AGD.Service.Shared.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Security.Claims;

namespace AGD.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "2")]
    [ApiController]
    public class BookmarkController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;
        public BookmarkController(IServicesProvider servicesProvider)
        {
            _servicesProvider = servicesProvider;
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var idString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")
                                            ?? User.Identity?.Name;
            return int.TryParse(idString, out userId);
        }

        [HttpPost("post/{postId:int}")]
        public async Task<ActionResult<ApiResult<string>>> AddPostBookmark([FromRoute] int postId, CancellationToken ct = default)
        {
            if (!TryGetUserId(out var userId))
            {
                return ApiResult<string>.FailResponse("Unauthorized", 401);
            }
            var result = await _servicesProvider.BookmarkService.AddPostBookmarkAsync(userId, postId, ct);
            if (!result)
            {
                return ApiResult<string>.FailResponse("Bookmark đã tồn tại hoặc không thể thêm bookmark.", 400);
            }
            return ApiResult<string>.SuccessResponse("Thêm bookmark thành công.");
        }

        [HttpDelete("posts/{postId:int}")]
        public async Task<ActionResult<ApiResult<string>>> RemovePostBookmark([FromRoute] int postId, CancellationToken ct = default)
        {
            if (!TryGetUserId(out var userId))
            {
                return ApiResult<string>.FailResponse("Unauthorized", 401);
            }
            var result = await _servicesProvider.BookmarkService.RemovePostBookmarkAsync(userId, postId, ct);
            if (!result)
            {
                return ApiResult<string>.FailResponse("Bookmark không tồn tại hoặc không thể xóa bookmark.", 400);
            }
            return ApiResult<string>.SuccessResponse("Xóa bookmark thành công.");
        }

        [HttpPost("restaurants/{restaurantId:int}")]
        public async Task<ActionResult<ApiResult<string>>> AddRestaurantBookmark([FromRoute] int restaurantId, CancellationToken ct)
        {
            if (!TryGetUserId(out var userId))
                return ApiResult<string>.FailResponse("Unauthorized", 401);

            var added = await _servicesProvider.BookmarkService.AddRestaurantBookmarkAsync(userId, restaurantId, ct);
            if (!added) return ApiResult<string>.FailResponse("Already bookmarked", 409);

            return ApiResult<string>.SuccessResponse("Bookmarked");
        }

        [HttpDelete("restaurants/{restaurantId:int}")]
        public async Task<ActionResult<ApiResult<string>>> RemoveRestaurantBookmark([FromRoute] int restaurantId, CancellationToken ct)
        {
            if (!TryGetUserId(out var userId))
                return ApiResult<string>.FailResponse("Unauthorized", 401);

            var removed = await _servicesProvider.BookmarkService.RemoveRestaurantBookmarkAsync(userId, restaurantId, ct);
            if (!removed) return ApiResult<string>.FailResponse("Not found", 404);

            return ApiResult<string>.SuccessResponse("Removed");
        }

        [HttpGet("posts")]
        [EnableQuery(PageSize = 20, MaxNodeCount = 80)]
        public ActionResult<IQueryable<Post>> GetBookmarkedPosts()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }
            var result = _servicesProvider.BookmarkService.QueryUserBookmarkedPosts(userId);
            return Ok(result);
        }

        [HttpGet("restaurants")]
        [EnableQuery(PageSize = 20, MaxNodeCount = 80)]
        public ActionResult<IQueryable<Restaurant>> GetBookmarkedRestaurants()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }
            var result = _servicesProvider.BookmarkService.QueryUserBookmarkedRestaurants(userId);
            return Ok(result);
        }

        [HttpGet("post/{postId:int}")]
        public async Task<ActionResult<ApiResult<Bookmark?>>> GetPostBookmark([FromRoute] int postId, CancellationToken ct = default)
        {
            if (!TryGetUserId(out var userId))
            {
                return ApiResult<Bookmark?>.FailResponse("Unauthorized", 401);
            }
            var bookmark = await _servicesProvider.BookmarkService.GetBookmarkByUserAndPostAsync(userId, postId, ct);
            return ApiResult<Bookmark?>.SuccessResponse(bookmark);
        }

        [HttpGet("restaurant/{restaurantId:int}")]
        public async Task<ActionResult<ApiResult<Bookmark?>>> GetRestaurantBookmark([FromRoute] int restaurantId, CancellationToken ct = default)
        {
            if (!TryGetUserId(out var userId))
            {
                return ApiResult<Bookmark?>.FailResponse("Unauthorized", 401);
            }
            var bookmark = await _servicesProvider.BookmarkService.GetBookmarkByUserAndRestaurantAsync(userId, restaurantId, ct);
            return ApiResult<Bookmark?>.SuccessResponse(bookmark);
        }
    }
}
