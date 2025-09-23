using AGD.Repositories.Models;
using AGD.Service.Services.Interfaces;
using AGD.Service.Shared.Result;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.Options;

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
    }
}
