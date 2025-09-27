using AGD.DAL.Basic;
using AGD.Repositories.DBContext;
using AGD.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace AGD.Repositories.Repositories
{
    public class RestaurantRepository : GenericRepository<Restaurant>
    {      
        public RestaurantRepository(AnGiDayContext context) : base(context)
        {
        }

        public IQueryable<Restaurant> SearchRestaurants(string? restaurantName, string? signatureFoodName)
        {
            IQueryable<Restaurant> query = _context.Restaurants
                .AsNoTracking()
                .Where(r => !r.IsDeleted);

            if (!string.IsNullOrWhiteSpace(restaurantName))
            {
                query = query.Where(r => EF.Functions.ILike(r.Name, $"%{restaurantName}%"));
            }

            if (!string.IsNullOrWhiteSpace(signatureFoodName))
            {
                query = query.Where(r => r.SignatureFoods.Any(sf => !sf.IsDeleted && EF.Functions.ILike(sf.Name, $"%{signatureFoodName}%")));
            }

            return query;
        }

        public async Task<IEnumerable<Restaurant>> GetRestaurantsByTags(List<int> tagId, CancellationToken ct)
        {
            var query = _context.Restaurants.AsNoTracking()
                .Where(rt => !rt.IsDeleted);

            if(tagId != null && tagId.Count != 0)
            {
                query = query.Where(r =>
                    r.RestaurantTags.Any(rt => tagId.Contains(rt.TagId)));
            }

            return await query.ToListAsync(ct);
        }

        public async Task<IEnumerable<SignatureFood>> GetRestaurantFood(int resId, CancellationToken ct)
        {
            var query = await _context.SignatureFoods.AsNoTracking().Where(f => f.RestaurantId == resId && !f.IsDeleted).ToListAsync(ct);

            return query;
        }
   
    }
}
