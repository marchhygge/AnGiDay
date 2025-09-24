using AGD.Repositories.Models;

namespace AGD.Service.Services.Interfaces
{
    public interface IBookmarkService
    {
        Task<bool> AddPostBookmarkAsync(int userId, int postId, CancellationToken ct = default);
        Task<bool> RemovePostBookmarkAsync(int userId, int postId, CancellationToken ct = default);
        Task<bool> AddRestaurantBookmarkAsync(int userId, int restaurantId, CancellationToken ct = default);
        Task<bool> RemoveRestaurantBookmarkAsync(int userId, int restaurantId, CancellationToken ct = default);
        Task<Bookmark?> GetBookmarkByUserAndPostAsync(int userId, int postId, CancellationToken ct = default);
        Task<Bookmark?> GetBookmarkByUserAndRestaurantAsync(int userId, int restaurantId, CancellationToken ct = default);
        IQueryable<Post> QueryUserBookmarkedPosts(int userId);
        IQueryable<Restaurant> QueryUserBookmarkedRestaurants(int userId);
    }
}
