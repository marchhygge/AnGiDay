using AGD.Repositories.Models;
using AGD.Service.Services.Interfaces;
using AGD.Service.Shared.Result;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace AGD.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantController : ODataController
    {
        private readonly IServicesProvider _servicesProvider;
        public RestaurantController(IServicesProvider servicesProvider)
        {
            _servicesProvider = servicesProvider;
        }

        [HttpGet("search-restaurant")]
        [EnableQuery(PageSize = 20, MaxNodeCount = 80)]
        public IQueryable<Restaurant> SearchRestaurants([FromQuery] string? restaurantName, [FromQuery] string? signatureFoodName)
        {
            return _servicesProvider.RestaurantService.SearchRestaurants(restaurantName, signatureFoodName);
        }

        [HttpGet("filter-restaurant")]
        [EnableQuery(PageSize = 20, MaxNodeCount = 80)]
        public IQueryable<Restaurant> GetRestaurantsByTags([FromQuery] List<int> tagIds)
        {
            return _servicesProvider.RestaurantService.GetRestaurantsByTags(tagIds);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<Restaurant?>>> GetRestaurantByIdAsync ([FromRoute] int id, CancellationToken ct)
        {
            var res = await _servicesProvider.RestaurantService.GetAsync(id, ct);

            if(res == null)
            {
                return ApiResult<Restaurant?>.FailResponse("Restaurant not exist");
            }

            return ApiResult<Restaurant?>.SuccessResponse(res);
        }

        [HttpGet("posts/{restaurantId:int}")]
        public async Task<ActionResult<IQueryable<Post>>> GetRestaurantPost([FromRoute] int restaurantId, CancellationToken ct = default)
        {
            var res = await _servicesProvider.RestaurantService.GetAsync(restaurantId, ct);

            if (res == null)
            {
                return BadRequest("Restaurant not exist");
            }
            return Ok(_servicesProvider.RestaurantService.GetRestaurantPost(restaurantId));
       
        }

        [HttpGet("signaturefood/{restaurantId:int}")]
        public async Task<ActionResult<IQueryable<Post>>> GetRestaurantFood([FromRoute] int restaurantId, CancellationToken ct = default)
        {
            var res = await _servicesProvider.RestaurantService.GetAsync(restaurantId, ct);

            if (res == null)
            {
                return BadRequest("Restaurant not exist");
            }
            return Ok(_servicesProvider.RestaurantService.GetRestaurantFood(restaurantId));
        }

        [HttpGet("feedback/{restaurantId:int}")]
        public async Task<ActionResult<IQueryable<Post>>> GetRestaurantFeedback([FromRoute] int restaurantId, CancellationToken ct = default)
        {
            var res = await _servicesProvider.RestaurantService.GetAsync(restaurantId, ct);

            if (res == null)
            {
                return BadRequest("Restaurant not exist");
            }

            var feedback = _servicesProvider.RestaurantService.GetRestaurantFeedback(restaurantId);
            return Ok(feedback);
        }



    }
}
