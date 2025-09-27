using AGD.Repositories.Repositories;
using AGD.Service.Integrations.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace AGD.Service.Services.Implement
{
    public class ContextBuilder
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWeatherProvider _weatherProvider;
        private readonly IConfiguration _configuration;

        public ContextBuilder(IUnitOfWork unitOfWork, IWeatherProvider weatherProvider, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _weatherProvider = weatherProvider ?? throw new ArgumentNullException(nameof(weatherProvider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<(string systemPrompt, List<(string role, string content)> recent)> BuildAsync(
            int userId,
            int conversationId,
            string latestUserMessage,
            IEnumerable<dynamic> restaurants,
            IEnumerable<dynamic> foods,
            IEnumerable<(string role, string content)> recentMessagesSnapshot,
            CancellationToken ct)
        {
            var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId, ct);
            var hp = await _unitOfWork.UserRepository.GetHealthProfileAsync(userId, ct);
            var userTags = await _unitOfWork.UserRepository.GetUserTagNamesAsync(userId, ct);
            var loc = await _unitOfWork.UserRepository.GetCurrentOrHomeLocationAsync(userId, ct);

            WeatherInfo? wx = null;
            if (loc != null)
            {
                int cacheMin = int.Parse(_configuration["Weather:CacheMinutes"] ?? "10");
                wx = await _weatherProvider.GetCurrentAsync(loc.Latitude, loc.Longitude, cacheMin, ct);
            }

            var sb = new StringBuilder();
            sb.AppendLine("Bạn là trợ lý gợi ý ăn uống cá nhân hoá của hệ thống AnGiDay.");
            if (user != null)
            {
                sb.AppendLine($"Người dùng: {user.FullName} ({user.Gender})");
            }
            if (hp != null)
            {
                if (!string.IsNullOrWhiteSpace(hp.DietType)) sb.AppendLine($"- Chế độ ăn: {hp.DietType}");
                if (!string.IsNullOrWhiteSpace(hp.HealthGoals)) sb.AppendLine($"- Mục tiêu: {hp.HealthGoals}");
                if (!string.IsNullOrWhiteSpace(hp.Allergies)) sb.AppendLine($"- Dị ứng: {hp.Allergies}");
            }
            if (userTags != null && userTags.Count > 0) sb.AppendLine($"- Sở thích: {string.Join(", ", userTags)}");
            if (loc != null) sb.AppendLine($"Vị trí gần nhất: {loc.Address} (lat={loc.Latitude}, lon={loc.Longitude})");
            if (wx != null) sb.AppendLine($"Thời tiết: {wx.ConditionSummary}, nhiệt độ {wx.TemperatureCelsius}°C.");

            sb.AppendLine("Danh sách nhà hàng đề xuất (tối đa 5):");
            int i = 1;
            foreach (var r in restaurants.Take(5))
            {
                var tagsArr = (r.Tags as IEnumerable<string>) ?? Enumerable.Empty<string>();
                sb.AppendLine($"[{i++}] {r.Name} | Tags: {string.Join(", ", tagsArr)} | Rating {r.Rating:F1} | Cách {r.DistanceKm:F1}km | Địa chỉ: {r.Address}");
            }

            sb.AppendLine("Món ăn tiêu biểu:");
            i = 1;
            foreach (var f in foods.Take(5))
            {
                sb.AppendLine($"[{i++}] {f.Name} tại {f.RestaurantName}");
            }

            sb.AppendLine("Yêu cầu trả lời: trả lời ngắn gọn, tôn trọng dị ứng, hỏi 1 câu làm rõ nếu cần. Ngôn ngữ: tiếng Việt.");

            var recentPairs = recentMessagesSnapshot?.ToList() ?? new List<(string, string)>();

            return (sb.ToString(), recentPairs);
        }
    }
}
