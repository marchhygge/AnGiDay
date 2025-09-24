using AGD.Repositories.Models;

namespace AGD.Service.Services.Interfaces
{
    public interface IRestaurantService
    {
        IQueryable<Restaurant> SearchRestaurants(string? restaurantName, string? signatureFoodName);
        Task<Restaurant?> GetAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(Restaurant entity, CancellationToken ct = default);
        Task UpdateAsync(Restaurant entity, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        IQueryable<Restaurant> GetRestaurantsByTags(List<int> tagIds);
    }
}
