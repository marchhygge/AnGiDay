using AGD.DAL.Basic;
using AGD.Repositories.DBContext;
using AGD.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace AGD.Repositories.Repositories
{
    public class UserRepository : GenericRepository<User>
    {
        public UserRepository(AnGiDayContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted, ct);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);
        }

        public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.GoogleId == googleId && !u.IsDeleted, ct);
        }

        public async Task<User?> GetUserAsync(int userId, CancellationToken ct = default)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);
        }

        public async Task<(int postCount, int restaurantBookmarkCount, int postBookmarkCount, int ownedRestaurantCount)>
            GetUserAggregatesAsync(int userId, CancellationToken ct = default)
        {
            var postCount = await _context.Posts
                .AsNoTracking()
                .CountAsync(p => p.UserId == userId && !p.IsDeleted, ct);

            var restaurantBookmarkCount = await _context.Bookmarks
                .AsNoTracking()
                .CountAsync(b => b.UserId == userId && b.RestaurantId != null && !b.IsDeleted, ct);

            var postBookmarkCount = await _context.Bookmarks
                .AsNoTracking()
                .CountAsync(b => b.UserId == userId && b.PostId != null && !b.IsDeleted, ct);

            var ownedRestaurantCount = await _context.Restaurants
                .AsNoTracking()
                .CountAsync(r => r.OwnerId == userId && !r.IsDeleted, ct);

            return (postCount, restaurantBookmarkCount, postBookmarkCount, ownedRestaurantCount);
        }

        public async Task<IEnumerable<Post>> GetCommunityPost(CancellationToken ct = default)
        {
            var query = await _context.Posts.AsNoTracking()
                .Where(p => !p.IsDeleted && p.Type.Equals("community_post"))
                .Include(p => p.User)
                .ToListAsync(ct);

            return query;
        }
    }
}
