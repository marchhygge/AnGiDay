using AGD.Repositories.Models;
using AGD.Repositories.Repositories;
using AGD.Service.Services.Interfaces;

namespace AGD.Service.Services.Implement
{
    public class BookmarkService : IBookmarkService
    {
        private readonly IUnitOfWork _unitOfWork;
        public BookmarkService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddPostBookmarkAsync(int userId, int postId, CancellationToken ct = default)
        {
            var existingBookmark = await _unitOfWork.BookmarkRepository.GetByUserAndPostAsync(userId, postId, ct);
            if (existingBookmark != null)
            {
                return false;
            }

            var newBookmark = new Bookmark
            {
                UserId = userId,
                PostId = postId,
                RestaurantId = null,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            await _unitOfWork.BookmarkRepository.CreateAsync(newBookmark, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> AddRestaurantBookmarkAsync(int userId, int restaurantId, CancellationToken ct = default)
        {
            var existingBookmark = await _unitOfWork.BookmarkRepository.GetByUserAndRestaurant(userId, restaurantId, ct);
            if (existingBookmark != null)
            {
                return false;
            }

            var newBookmark = new Bookmark
            {
                UserId = userId,
                PostId = null,
                RestaurantId = restaurantId,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            await _unitOfWork.BookmarkRepository.CreateAsync(newBookmark, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }

        public async Task<Bookmark?> GetBookmarkByUserAndPostAsync(int userId, int postId, CancellationToken ct = default)
        {
            return await _unitOfWork.BookmarkRepository.GetByUserAndPostAsync(userId, postId, ct);
        }

        public async Task<Bookmark?> GetBookmarkByUserAndRestaurantAsync(int userId, int restaurantId, CancellationToken ct = default)
        {
            return await _unitOfWork.BookmarkRepository.GetByUserAndRestaurant(userId, restaurantId, ct);
        }

        public IQueryable<Post> QueryUserBookmarkedPosts(int userId)
        {
            return _unitOfWork.BookmarkRepository.QueryUserBookmarkedPosts(userId);
        }

        public IQueryable<Restaurant> QueryUserBookmarkedRestaurants(int userId)
        {
            return _unitOfWork.BookmarkRepository.QueryUserBookmarkedRestaurants(userId);
        }

        public async Task<bool> RemovePostBookmarkAsync(int userId, int postId, CancellationToken ct = default)
        {
            var existingBookmark = await _unitOfWork.BookmarkRepository.GetByUserAndPostAsync(userId, postId, ct);
            if (existingBookmark == null)
            {
                return false;
            }

            await _unitOfWork.BookmarkRepository.DeleteAsync(existingBookmark, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> RemoveRestaurantBookmarkAsync(int userId, int restaurantId, CancellationToken ct = default)
        {
            var existingBookmark = await _unitOfWork.BookmarkRepository.GetByUserAndRestaurant(userId, restaurantId, ct);
            if (existingBookmark == null)
            {
                return false;
            }

            await _unitOfWork.BookmarkRepository.DeleteAsync(existingBookmark, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }
    }
}
