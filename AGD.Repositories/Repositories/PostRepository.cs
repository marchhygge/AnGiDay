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
                .FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted, ct);

            return query;
        }

        public async Task<UserPostInteraction?> GetByUserAndPostId (int userId, int postId, CancellationToken ct)
        {
            return await _context.UserPostInteractions.AsNoTracking()
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PostId == postId && !up.IsDeleted, ct);
        }

        public async Task<UserPostInteraction> AddInteractionAsync (UserPostInteraction interaction, CancellationToken ct)
        {
            _context.UserPostInteractions.Add(interaction);
            await _context.SaveChangesAsync(ct);
            return interaction;
        }

        public async Task<UserPostInteraction> UpdateInteractionAsync(UserPostInteraction interaction, CancellationToken ct)
        {
            _context.UserPostInteractions.Update(interaction);
            await _context.SaveChangesAsync(ct);
            return interaction;
        }
    }
}
