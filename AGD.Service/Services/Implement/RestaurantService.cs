using AGD.Repositories.Models;
using AGD.Repositories.Repositories;
using AGD.Service.DTOs.Response;
using AGD.Service.Services.Interfaces;

namespace AGD.Service.Services.Implement
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IUnitOfWork _unitOfWork;
        public RestaurantService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IQueryable<Restaurant> SearchRestaurants(string? restaurantName, string? signatureFoodName)
        {
            restaurantName = string.IsNullOrWhiteSpace(restaurantName) ? null : restaurantName.Trim();
            signatureFoodName = string.IsNullOrWhiteSpace(signatureFoodName) ? null : signatureFoodName.Trim();

            return _unitOfWork.RestaurantRepository.SearchRestaurants(restaurantName, signatureFoodName);
        }

        public async Task<int> CreateAsync(Restaurant entity, CancellationToken ct = default)
        {
            await _unitOfWork.RestaurantRepository.CreateAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var restaurant = await _unitOfWork.RestaurantRepository.GetByIdAsync(ct, id);
            if (restaurant == null) return;
            await _unitOfWork.RestaurantRepository.DeleteAsync(restaurant, ct);
        }

        public async Task<Restaurant?> GetAsync(int id, CancellationToken ct = default)
        {
            return await _unitOfWork.RestaurantRepository.GetByIdAsync(ct, id);
        }

        public async Task UpdateAsync(Restaurant entity, CancellationToken ct = default)
        {
            await _unitOfWork.RestaurantRepository.UpdateAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<Restaurant>> GetRestaurantsByTags(List<int> tagIds, CancellationToken ct = default)
        {
            return await _unitOfWork.RestaurantRepository.GetRestaurantsByTags(tagIds, ct);
        }

        public async Task<IEnumerable<SignatureFood>> GetRestaurantFood(int resId, CancellationToken ct = default)
        {
            return await _unitOfWork.RestaurantRepository.GetRestaurantFood(resId, ct);
        }        
    }
}
