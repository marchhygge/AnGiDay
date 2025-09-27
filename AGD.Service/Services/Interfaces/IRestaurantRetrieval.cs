namespace AGD.Service.Services.Interfaces
{
    public interface IRestaurantRetrieval
    {
        Task<(List<RankedRestaurant> Restaurants, List<RankedFood> Foods)> FindCandidatesAsync(
            int userId,
            string userQuery,
            double? userLat,
            double? userLon,
            int topK,
            CancellationToken ct = default);
    }
    public record RankedRestaurant(
        int Id,
        string Name,
        string[] Tags,
        double Rating,
        double DistanceKm,
        string Address
    )
    {
        public double Score { get; init; } = 0;
    }

    public record RankedFood(
        int Id,
        string Name,
        int RestaurantId,
        string RestaurantName,
        string[] Tags
    );
}