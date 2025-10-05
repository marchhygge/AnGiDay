using AGD.Service.Integrations.Interfaces;

namespace AGD.Service.Services.Interfaces
{
    public interface IServicesProvider
    {
        IRestaurantService RestaurantService { get; }
        IUserService UserService { get; }
        IObjectStorageService ObjectStorageService { get; }
        IBookmarkService BookmarkService { get; }
        IPostService PostService { get; }    
        ITokenBlacklistService TokenBlacklistService { get; }
        ITokenService TokenService { get; }
        IChatService ChatService { get; }
        IWeatherProvider WeatherProvider { get; }
        ITagService TagService { get; }
        IUserTagService UserTagService { get; }
    }
}
