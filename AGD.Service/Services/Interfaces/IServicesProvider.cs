namespace AGD.Service.Services.Interfaces
{
    public interface IServicesProvider
    {
        IRestaurantService RestaurantService { get; }
        IUserService UserService { get; }
        IObjectStorageService ObjectStorageService { get; }
        IBookmarkService BookmarkService { get; }
        IPostService PostService { get; }
        }
}
