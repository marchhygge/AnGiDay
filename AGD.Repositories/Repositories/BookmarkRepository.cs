using AGD.DAL.Basic;
using AGD.Repositories.DBContext;
using AGD.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace AGD.Repositories.Repositories
{
    public class BookmarkRepository : GenericRepository<Bookmark>
    {
        public BookmarkRepository(AnGiDayContext context) : base(context)
        {
        }

        public async Task<Bookmark?> GetByUserAndPostAsync(int userId, int postId, CancellationToken ct = default)
        {
            return await _context.Bookmarks.AsNoTracking()
                .FirstOrDefaultAsync(b => b.UserId == userId && b.PostId == postId && !b.IsDeleted, ct);
        }

        public async Task<Bookmark?> GetByUserAndRestaurant(int userId, int restaurantId, CancellationToken ct = default)
        {
            return await _context.Bookmarks.AsNoTracking()
                .FirstOrDefaultAsync(b => b.UserId == userId && b.RestaurantId == restaurantId && !b.IsDeleted, ct);
        }

        public IQueryable<Post> QueryUserBookmarkedPosts(int userId)
        {
            return _context.Bookmarks.AsNoTracking()
                .Where(b => b.UserId == userId && b.PostId != null && !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => b.Post!)
                .Where(p => !p.IsDeleted);
        }

        public IQueryable<Restaurant> QueryUserBookmarkedRestaurants(int userId)
        {
            return _context.Bookmarks.AsNoTracking()
                .Where(b => b.UserId == userId && b.RestaurantId != null && !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => b.Restaurant!)
                .Where(r => !r.IsDeleted);
        }
    }
}
