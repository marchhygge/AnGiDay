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
    }
}
