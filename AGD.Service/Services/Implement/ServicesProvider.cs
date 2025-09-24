using AGD.Repositories.ConfigurationModels;
using AGD.Repositories.Repositories;
using AGD.Service.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AGD.Service.Services.Implement
{
    public class ServicesProvider : IServicesProvider
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<GoogleIdTokenOptions> _googleOptions;
        private readonly IOptions<R2Options> _r2Options;
        private IRestaurantService? _restaurantService;
        private IUserService? _userService;
        private IObjectStorageService? _objectStorageService;
        private IBookmarkService? _bookmarkService;

        public ServicesProvider(IUnitOfWork unitOfWork, IOptions<GoogleIdTokenOptions> googleOptions, IOptions<R2Options> r2Options)
        {
            _unitOfWork = unitOfWork;
            _googleOptions = googleOptions;
            _r2Options = r2Options;
        }
        public IRestaurantService RestaurantService => _restaurantService ??= new RestaurantService(_unitOfWork);
        public IUserService UserService => _userService ??= new UserService(_unitOfWork, _googleOptions);
        public IObjectStorageService ObjectStorageService => _objectStorageService ??= new R2StorageService(_r2Options);
        public IBookmarkService BookmarkService => _bookmarkService ??= new BookmarkService(_unitOfWork);
    }
}
