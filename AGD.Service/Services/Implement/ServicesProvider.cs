using AGD.Repositories.Repositories;
using AGD.Service.Services.Interfaces;

namespace AGD.Service.Services.Implement
{
    public class ServicesProvider : IServicesProvider
    {
        private readonly IUnitOfWork _unitOfWork;
        private IRestaurantService? _restaurantService;

        public ServicesProvider(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IRestaurantService RestaurantService => _restaurantService ??= new RestaurantService(_unitOfWork);
    }
}
