using AGD.DAL.Basic;
using AGD.Repositories.DBContext;
using AGD.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace AGD.Repositories.Repositories
{
    public sealed record RestaurantBasicProjection(
        int Id,
        string Name,
        string Address,
        double Latitude,
        double Longitude,
        double? AvgRating,
        int? RatingCount
    );

    public sealed record FoodBasicProjection(
        int Id,
        string Name,
        int RestaurantId,
        string RestaurantName
    );

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

            if (tagId != null && tagId.Count != 0)
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

        public IQueryable<Post> GetRestaurantFeedback(int resId)
        {
            IQueryable<Post> query = _context.Posts.AsNoTracking()
                .Where(p => p.RestaurantId == resId && !p.IsDeleted && p.Type.Equals("review"))
                .Include(p => p.User)
                .Include(p => p.SignatureFood)
                .AsQueryable();

            return query;
        }

        public async Task<List<Restaurant>> GetRestaurantsToEmbedAsync(IEnumerable<int> excludeIds, int take, CancellationToken ct = default)
        {
            var excludes = (excludeIds ?? Array.Empty<int>()).ToList();
            var q = _context.Restaurants
                .AsNoTracking()
                .Where(r => (r.IsDeleted == false) && r.Status == "active");

            if (excludes.Count > 0)
                q = q.Where(r => !excludes.Contains(r.Id));

            return await q.OrderBy(r => r.Id).Take(take).ToListAsync(ct);
        }

        public async Task<string> BuildTextForEmbeddingAsync(int restaurantId, CancellationToken ct = default)
        {
            var r = await _context.Restaurants
                .AsNoTracking()
                .Where(x => x.Id == restaurantId)
                .Select(x => new { x.Name, x.Description })
                .FirstOrDefaultAsync(ct);

            if (r == null) return string.Empty;

            var tags = await (from rt in _context.RestaurantTags
                              join t in _context.Tags on rt.TagId equals t.Id
                              where rt.RestaurantId == restaurantId && (rt.IsDeleted == null || rt.IsDeleted == false) && (t.IsDeleted == false)
                              select t.Name).Distinct().ToListAsync(ct);

            var sb = new System.Text.StringBuilder();
            sb.Append(r.Name);
            if (!string.IsNullOrWhiteSpace(r.Description))
            {
                sb.Append(". ");
                sb.Append(r.Description);
            }
            if (tags.Count > 0)
            {
                sb.Append(". Tags: ");
                sb.Append(string.Join(", ", tags));
            }
            return sb.ToString();
        }

        public async Task<List<Restaurant>> GetRestaurantsByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids?.ToList() ?? new List<int>();
            if (idList.Count == 0) return new List<Restaurant>();

            return await _context.Restaurants
                .AsNoTracking()
                .Where(r => idList.Contains(r.Id) && (r.IsDeleted == false) && r.Status == "active")
                .ToListAsync(ct);
        }

        public async Task<List<RestaurantBasicProjection>> GetActiveRestaurantsBasicAsync(CancellationToken ct = default)
        {
            return await _context.Restaurants
                .AsNoTracking()
                .Where(r => r.IsDeleted == false && r.Status == "active")
                .Select(r => new RestaurantBasicProjection(
                    r.Id,
                    r.Name,
                    r.Address,
                    r.Latitude,
                    r.Longitude,
                    r.AvgRating.HasValue ? (double?)r.AvgRating.Value : null,
                    r.RatingCount
                ))
                .ToListAsync(ct);
        }

        public async Task<Dictionary<int, List<string>>> GetTagNamesForRestaurantsAsync(IEnumerable<int> restaurantIds, CancellationToken ct = default)
        {
            var ids = restaurantIds?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0) return new Dictionary<int, List<string>>();

            var pairs = await (from rt in _context.RestaurantTags
                               join t in _context.Tags on rt.TagId equals t.Id
                               where ids.Contains(rt.RestaurantId)
                                     && (rt.IsDeleted == null || rt.IsDeleted == false)
                                     && t.IsDeleted == false
                               select new { rt.RestaurantId, t.Name })
                               .AsNoTracking()
                               .ToListAsync(ct);

            return pairs
                .GroupBy(x => x.RestaurantId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList());
        }

        public async Task<List<FoodBasicProjection>> GetSignatureFoodsForRestaurantsAsync(IEnumerable<int> restaurantIds, CancellationToken ct = default)
        {
            var ids = restaurantIds?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0) return new List<FoodBasicProjection>();

            return await (from f in _context.SignatureFoods
                          join r in _context.Restaurants on f.RestaurantId equals r.Id
                          where ids.Contains(f.RestaurantId) && f.IsDeleted == false && r.IsDeleted == false
                          select new FoodBasicProjection(f.Id, f.Name, f.RestaurantId, r.Name))
                         .AsNoTracking()
                         .ToListAsync(ct);
        }

        public async Task<Dictionary<int, double>> GetRatingsForRestaurantsAsync(IEnumerable<int> restaurantIds, CancellationToken ct)
        {
            return await _context.UserRestaurantInteractions
                .Where(ur => restaurantIds.Contains(ur.RestaurantId!.Value) && ur.Rating.HasValue && !ur.IsDeleted)
                .GroupBy(ur => ur.RestaurantId)
                .Select(g => new { RestaurantId = g.Key!.Value, AvgRating = g.Average(x => (double)x.Rating!.Value) })
                .ToDictionaryAsync(x => x.RestaurantId, x => x.AvgRating, cancellationToken: ct);
        }
    }
}
