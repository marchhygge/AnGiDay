using AGD.DAL.Basic;
using AGD.Repositories.DBContext;
using AGD.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Repositories.Repositories
{
    public class PostRepository : GenericRepository<Post>
    {
        public PostRepository(AnGiDayContext context) : base(context)
        {
        }

        public IQueryable<Post> GetRestaurantPost(int resId)
        {
            IQueryable<Post> query = _context.Posts.AsNoTracking().Where(p => p.RestaurantId == resId && !p.IsDeleted && p.Type.Equals("owner_post")).AsQueryable();

            return query;
        }    

        public async Task<IEnumerable<Post>> GetRestaurantFeedback(int resId, CancellationToken ct)
        {
            var query = await _context.Posts.AsNoTracking()
                .Where(p => p.RestaurantId == resId && !p.IsDeleted && p.Type.Equals("review"))
                .Include(p => p.User)
                .Include(p => p.SignatureFood).ToListAsync(ct);

            return query;
        }

        public async Task<Post?> GetPostDetailAsync (int postId, CancellationToken ct)
        {
            var query = await _context.Posts.AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.InverseParent)
                    .ThenInclude(c => c.User)
                .Include(p => p.Restaurant)
                .FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted, ct);

            return query;
        }

        public async Task<Like?> GetByUserAndPostId (int userId, int postId, CancellationToken ct)
        {
            return await _context.Likes.AsNoTracking()
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PostId == postId, ct);
        }

        public async Task<int> CountLikesByPostIdAsync(int postId, CancellationToken ct)
        {
            return await _context.Likes.AsNoTracking()
                .Where(l => l.PostId == postId && !l.IsDeleted)
                .CountAsync(ct);
        }

        public async Task<Like> AddLikePost(Like like, CancellationToken ct)
        {
            _context.Likes.Add(like);
            await SaveChangesAsync(ct);
            return like;
        }

        public async Task<Like> UpdateLikePost(Like like, CancellationToken ct)
        {
            _context.Likes.Update(like);
            await SaveChangesAsync(ct);
            return like;
        }

        public async Task<Post> CreatePostAsync(Post post, CancellationToken ct)
        {
            _context.Posts.Add(post);
            await SaveChangesAsync(ct);
            return post;
        }

        //public async Task<Post?> GetPostByIdAsync(int id, CancellationToken ct)
        //{
        //    return await _context.Posts
        //        .Include(p => p.User)
        //        .Include(p => p.Restaurant)
        //        .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        //}

        public async Task<IEnumerable<Post>> GetPostsByTypeAsync(string type, CancellationToken ct)
        {
            return await _context.Posts
                .Where(p => p.Type == type && !p.IsDeleted)
                .Include(p => p.User)
                .Include(p => p.Restaurant)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<Post> UpdatePostAsync(Post post, CancellationToken ct)
        {
            _context.Posts.Update(post);
            await SaveChangesAsync(ct);
            return post;
        }

        public async Task<Post> SoftDeletePostAsync(Post post, CancellationToken ct)
        {
            post.IsDeleted = true;
            post.UpdatedAt = DateTime.Now;
            _context.Posts.Update(post);
            await SaveChangesAsync(ct);
            return post;
        }
    }
}
