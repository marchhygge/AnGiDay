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
    public class UserTagRepository : GenericRepository<UserTag>
    {
        public UserTagRepository(AnGiDayContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserTag>> GetTagsOfUserAsync(int userId, CancellationToken ct)
        {
            return await _context.UserTags.AsNoTracking()
                .Where(ut => ut.UserId == userId && ut.IsDeleted == false)
                .Include(ut => ut.Tag)
                .ToListAsync(ct);
        }

        public async Task<UserTag?> GetUserTagAsync(int userId, int tagId, CancellationToken ct)
        {
            return await _context.UserTags.AsNoTracking()
                .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TagId == tagId, ct);
        }

        public async Task<UserTag> AddUserTagAsync(UserTag userTag, CancellationToken ct)
        {
            _context.UserTags.Add(userTag);
            await SaveChangesAsync(ct);
            return userTag;
        }

        public async Task<UserTag> UpdateUserTagAsync(UserTag userTag, CancellationToken ct)
        {
            _context.UserTags.Update(userTag);
            await SaveChangesAsync(ct);
            return userTag;
        }

    }
}
