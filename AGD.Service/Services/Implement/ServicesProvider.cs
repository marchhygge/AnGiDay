using AGD.Repositories.ConfigurationModels;
using AGD.Repositories.Repositories;
using AGD.Service.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AGD.Service.Services.Implement
{
    public class ServicesProvider : IServicesProvider
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<GoogleIdTokenOptions> _options;
        private IRestaurantService? _restaurantService;
        private IUserService? _userService;

        public ServicesProvider(IUnitOfWork unitOfWork, IOptions<GoogleIdTokenOptions> options)
        {
            _unitOfWork = unitOfWork;
            _options = options;
        }
        public IRestaurantService RestaurantService => _restaurantService ??= new RestaurantService(_unitOfWork);
        public IUserService UserService => _userService ??= new UserService(_unitOfWork, _options);
    }
}
