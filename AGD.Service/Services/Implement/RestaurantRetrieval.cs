using AGD.Repositories.Repositories;
using AGD.Service.Services.Interfaces;

namespace AGD.Service.Services.Implement
{
    // Fallback retrieval based on tags + rating + distance (Haversine)
    public class RestaurantRetrieval : IRestaurantRetrieval
    {
        private readonly IUnitOfWork _unitOfWork;

        public RestaurantRetrieval(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(List<RankedRestaurant> Restaurants, List<RankedFood> Foods)> FindCandidatesAsync(
            int userId, string userQuery, double? userLat, double? userLon, int topK, CancellationToken ct)
        {
            var userTagNames = await _unitOfWork.UserRepository.GetUserTagNamesAsync(userId, ct);

            if (!userLat.HasValue || !userLon.HasValue)
            {
                var loc = await _unitOfWork.UserRepository.GetCurrentOrHomeLocationAsync(userId, ct);
                if (loc != null)
                {
                    userLat = loc.Latitude;
                    userLon = loc.Longitude;
                }
            }

            var baseList = await _unitOfWork.RestaurantRepository.GetActiveRestaurantsBasicAsync(ct);
            var tagDict = await _unitOfWork.RestaurantRepository.GetTagNamesForRestaurantsAsync(baseList.Select(b => b.Id), ct);

            double Haversine(double lat1, double lon1, double lat2, double lon2)
            {
                const double R = 6371.0;
                double dLat = (lat2 - lat1) * Math.PI / 180.0;
                double dLon = (lon2 - lon1) * Math.PI / 180.0;
                lat1 *= Math.PI / 180.0; lat2 *= Math.PI / 180.0;
                double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
                double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                return R * c;
            }

            var scored = baseList.Select(x =>
            {
                double distance = (userLat.HasValue && userLon.HasValue) ? Haversine(userLat.Value, userLon.Value, x.Latitude, x.Longitude) : 999;
                var tags = tagDict.TryGetValue(x.Id, out var tagList) ? tagList.ToArray() : Array.Empty<string>();
                double tagMatch = 0;
                if (tags.Length > 0 && userTagNames.Count > 0)
                {
                    var inter = tags.Intersect(userTagNames, StringComparer.OrdinalIgnoreCase).Count();
                    tagMatch = inter / (double)tags.Length;
                }

                double avgRating = x.AvgRating.GetValueOrDefault(0.0);
                double ratingScore = avgRating / 5.0;
                double distanceScore = 1.0 - Math.Min(distance / 8.0, 1.0);

                double score = 0.35 * tagMatch + 0.35 * ratingScore + 0.30 * distanceScore;

                return new RankedRestaurant(x.Id, x.Name, tags, avgRating, distance, x.Address) { Score = score };
            })
            .OrderByDescending(r => r.Score)
            .Take(topK)
            .ToList();

            var topIds = scored.Select(r => r.Id).ToList();

            var foodBasics = await _unitOfWork.RestaurantRepository.GetSignatureFoodsForRestaurantsAsync(topIds, ct);
            var foods = foodBasics
                .Select(f => new RankedFood(f.Id, f.Name, f.RestaurantId, f.RestaurantName, Array.Empty<string>()))
                .ToList();

            return (scored, foods);
        }
    }
}