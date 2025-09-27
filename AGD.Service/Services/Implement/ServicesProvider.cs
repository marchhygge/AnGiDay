using AGD.Repositories.ConfigurationModels;
using AGD.Repositories.Repositories;
using AGD.Service.Integrations;
using AGD.Service.Integrations.Implements;
using AGD.Service.Integrations.Interfaces;
using AGD.Service.Services.Interfaces;
using AGD.Service.Services.Retrieval;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace AGD.Service.Services.Implement
{
    public class ServicesProvider : IServicesProvider
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<GoogleIdTokenOptions> _googleOptions;
        private readonly IOptions<R2Options> _r2Options;
        private readonly IOptions<JwtSettings> _jwtOptions;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IEmailService _emailService;
        private readonly IRestaurantRetrieval _restaurantRetrieval;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<OpenMeteoWeatherProvider> _logger;
        private readonly VectorRetrievalService _vectorRetrievalService;
        private readonly ContextBuilder _contextBuilder;
        private readonly OllamaClient _ollamaClient;
        private readonly HttpClient _httpClient;
        private IRestaurantService? _restaurantService;
        private IUserService? _userService;
        private IObjectStorageService? _objectStorageService;
        private IBookmarkService? _bookmarkService;
        private ITokenBlacklistService? _tokenBlacklistService;
        private ITokenService? _tokenService;
        private IChatService? _chatService;
        private IWeatherProvider? _weatherProvider;

        public ServicesProvider(IUnitOfWork unitOfWork, 
                                IOptions<GoogleIdTokenOptions> googleOptions, 
                                IOptions<R2Options> r2Options, 
                                IOptions<JwtSettings> jwtOptions, 
                                IEmailService emailService,
                                IConnectionMultiplexer connectionMultiplexer,
                                IRestaurantRetrieval restaurantRetrieval,
                                IConfiguration configuration,
                                IDistributedCache distributedCache,
                                ILogger<OpenMeteoWeatherProvider> logger,
                                VectorRetrievalService vectorRetrievalService,
                                ContextBuilder contextBuilder,
                                OllamaClient ollamaClient,
                                HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _googleOptions = googleOptions;
            _r2Options = r2Options;
            _jwtOptions = jwtOptions;
            _emailService = emailService;
            _connectionMultiplexer = connectionMultiplexer;
            _restaurantRetrieval = restaurantRetrieval;
            _configuration = configuration;
            _distributedCache = distributedCache;
            _logger = logger;
            _vectorRetrievalService = vectorRetrievalService;
            _contextBuilder = contextBuilder;
            _ollamaClient = ollamaClient;
            _httpClient = httpClient;
        }
        public IRestaurantService RestaurantService => _restaurantService ??= new RestaurantService(_unitOfWork);
        public IObjectStorageService ObjectStorageService => _objectStorageService ??= new R2StorageService(_r2Options);
        public IUserService UserService => _userService ??= new UserService(_unitOfWork, _googleOptions, _emailService, _jwtOptions, _objectStorageService!);
        public IBookmarkService BookmarkService => _bookmarkService ??= new BookmarkService(_unitOfWork);
        public ITokenBlacklistService TokenBlacklistService => _tokenBlacklistService ??= new RedisTokenBlacklistService(_connectionMultiplexer);
        public ITokenService TokenService => _tokenService ??= new TokenService(_unitOfWork.JwtHelper);
        public IWeatherProvider WeatherProvider => _weatherProvider ??= new OpenMeteoWeatherProvider(_httpClient, _distributedCache, _logger);
        public IChatService ChatService => _chatService ??= new ChatService(_unitOfWork, _restaurantRetrieval, _vectorRetrievalService, _contextBuilder, _ollamaClient, _configuration);
    }
}
