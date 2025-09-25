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

        public IQueryable<Restaurant> GetRestaurantsByTags(List<int> tagId)
        {
            IQueryable<Restaurant> query = _context.Restaurants.AsNoTracking()
                .Where(rt => !rt.IsDeleted)
                .AsQueryable();

            if(tagId != null && tagId.Count != 0)
            {
                query = query.Where(r =>
                    r.RestaurantTags.Any(rt => tagId.Contains(rt.TagId)));
            }

            return query;
        }

        public IQueryable<Post> GetRestaurantPost(int resId)
        {
            IQueryable<Post> query = _context.Posts.AsNoTracking().Where(p => p.RestaurantId == resId && !p.IsDeleted && p.Type.Equals("owner_post")).AsQueryable();

            return query;
        }

        public IQueryable<SignatureFood> GetRestaurantFood(int resId)
        {
            IQueryable<SignatureFood> query = _context.SignatureFoods.AsNoTracking().Where(f => f.RestaurantId == resId && !f.IsDeleted).AsQueryable();

            return query;
        }

        public IQueryable<Post> GetRestaurantFeedback(int resId)
        {
            IQueryable<Post> query = _context.Posts.AsNoTracking()
                .Where(p => p.RestaurantId == resId && !p.IsDeleted && p.Type.Equals("review"))
                .Include(p => p.User)
                .Include(p => p.SignatureFood)
                .AsQueryable();

            return query;
        }
    }
}
