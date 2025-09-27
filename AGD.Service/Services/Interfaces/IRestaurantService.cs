using AGD.Repositories.Models;
using AGD.Service.DTOs.Response;

namespace AGD.Service.Services.Interfaces
{
    public interface IRestaurantService
    {
        IQueryable<Restaurant> SearchRestaurants(string? restaurantName, string? signatureFoodName);
        Task<Restaurant?> GetAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(Restaurant entity, CancellationToken ct = default);
        Task UpdateAsync(Restaurant entity, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<Restaurant>> GetRestaurantsByTags(List<int> tagIds, CancellationToken ct = default);
        //IQueryable<Post> GetRestaurantPost(int resId);
        Task<IEnumerable<SignatureFood>> GetRestaurantFood(int resId, CancellationToken ct = default);
        //Task<IEnumerable<FeedbackResponse>> GetRestaurantFeedback(int resId, CancellationToken ct = default);

    }
}
