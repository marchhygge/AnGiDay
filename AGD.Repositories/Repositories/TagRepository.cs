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
    public class TagRepository : GenericRepository<Tag>
    {
        public TagRepository(AnGiDayContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Tag>> GetTagAsync (CancellationToken ct = default)
        {
            return await _context.Tags.AsNoTracking()
                .Where(t => !t.IsDeleted)
                .Include(t => t.Category)
                .ToListAsync(ct);
        }
    }
}
