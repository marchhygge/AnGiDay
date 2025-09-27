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
        private readonly IOptions<JwtSettings> _jwtOptions;
        private readonly IEmailService _emailService;
        private IRestaurantService? _restaurantService;
        private IUserService? _userService;
        private IObjectStorageService? _objectStorageService;
        private IBookmarkService? _bookmarkService;
        private IPostService? _postService;

        public ServicesProvider(IUnitOfWork unitOfWork, 
                                IOptions<GoogleIdTokenOptions> googleOptions, 
                                IOptions<R2Options> r2Options, 
                                IOptions<JwtSettings> jwtOptions, 
                                IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _googleOptions = googleOptions;
            _r2Options = r2Options;
            _jwtOptions = jwtOptions;
            _emailService = emailService;
        }
        public IRestaurantService RestaurantService => _restaurantService ??= new RestaurantService(_unitOfWork);
        public IObjectStorageService ObjectStorageService => _objectStorageService ??= new R2StorageService(_r2Options);
        public IUserService UserService => _userService ??= new UserService(_unitOfWork, _googleOptions, _emailService, _jwtOptions, _objectStorageService!);
        public IBookmarkService BookmarkService => _bookmarkService ??= new BookmarkService(_unitOfWork);
        public IPostService PostService => _postService ??= new PostService(_unitOfWork);
    }
}
