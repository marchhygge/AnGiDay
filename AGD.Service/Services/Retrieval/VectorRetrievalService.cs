using AGD.Repositories.Repositories;

namespace AGD.Service.Services.Retrieval
{
    public class VectorRetrievalService
    {
        private readonly IUnitOfWork _unitOfWork;
        public VectorRetrievalService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<(int Id, string Name, double Lat, double Lon)>> RetrieveTopRestaurantsAsync(float[] queryVec, int topK, CancellationToken ct)
        {
            var ids = await _unitOfWork.EmbeddingRepository.TopRestaurantIdsByEmbeddingAsync(queryVec, topK, ct);
            if (ids == null || ids.Count == 0) return new List<(int, string, double, double)>();

            var restaurants = await _unitOfWork.RestaurantRepository.GetRestaurantsByIdsAsync(ids, ct);

            var idToRest = restaurants.ToDictionary(r => r.Id);
            var ordered = ids.Where(idToRest.ContainsKey)
                             .Select(id => {
                                 var r = idToRest[id];
                                 return (r.Id, r.Name, r.Latitude, r.Longitude);
                             }).ToList();

            return ordered;
        }
    }
}
