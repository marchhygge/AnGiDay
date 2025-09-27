using AGD.Repositories.Models;
using AGD.Service.DTOs.Response;
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
        public Task<IEnumerable<Restaurant>> GetRestaurantsByTags([FromQuery] List<int> tagIds, CancellationToken ct)
        {
            return _servicesProvider.RestaurantService.GetRestaurantsByTags(tagIds, ct);
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

        [HttpGet("signaturefood/{restaurantId:int}")]
        public async Task<ActionResult<ApiResult<IEnumerable<SignatureFood?>>>> GetRestaurantFood([FromRoute] int restaurantId, CancellationToken ct = default)
        {
            var res = await _servicesProvider.RestaurantService.GetAsync(restaurantId, ct);

            if (res == null)
            {
                return ApiResult<IEnumerable<SignatureFood?>>.FailResponse("Restaurant not exist");
            }
            var food = await _servicesProvider.RestaurantService.GetRestaurantFood(restaurantId, ct);
            return ApiResult<IEnumerable<SignatureFood?>>.SuccessResponse(food);
        }
    }
}
