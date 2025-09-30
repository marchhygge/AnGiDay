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

            sb.AppendLine("Bạn là trợ lý AI của hệ thống AnGiDay. Nhiệm vụ duy nhất của bạn là đề xuất món ăn và nhà hàng cụ thể phù hợp với yêu cầu của người dùng trong phạm vi hệ thống AnGiDay.");
            sb.AppendLine("LUÔN tuân thủ các quy tắc sau (bắt buộc):");
            sb.AppendLine("1) Chỉ trả lời các câu hỏi liên quan trực tiếp đến: món ăn, nhà hàng, chi nhánh, địa chỉ, khoảng cách, đánh giá, thực đơn, chế độ ăn (khi có dữ liệu), dị ứng (khi có dữ liệu), vị trí người dùng trong hệ thống AnGiDay.");
            sb.AppendLine("2) Tuyệt đối KHÔNG trả lời các câu hỏi ngoài phạm vi trên. Nếu câu hỏi không liên quan, trả lời chính xác duy nhất dòng sau và KHÔNG thêm nội dung gì khác:");
            sb.AppendLine("'Xin lỗi, tôi chỉ hỗ trợ các câu hỏi về ăn uống và nhà hàng trong hệ thống AnGiDay.'");
            sb.AppendLine("3) Không tự động đưa thông tin y tế chuyên sâu, không chẩn đoán bệnh và không đưa lời khuyên y tế. Nếu người dùng hỏi điều y tế chuyên sâu, trả lời theo quy tắc ở (2).");
            sb.AppendLine("4) Không hỏi lại người dùng. Nếu có dữ liệu nhà hàng/món phù hợp, luôn đưa ra danh sách gợi ý; nếu không có, nói rõ 'Không có gợi ý nào phù hợp.'");
            sb.AppendLine("5) Định dạng trả lời bắt buộc: mỗi gợi ý trên một dòng duy nhất, chính xác theo mẫu:");
            sb.AppendLine("[Tên món] tại [Tên nhà hàng] - Địa chỉ: [Địa chỉ], Cách: [Khoảng cách]km, Đánh giá: [Rating]");
            sb.AppendLine("6) Ngôn ngữ trả lời: Tiếng Việt. KHÔNG thêm chú thích, ký tự trang trí, lập danh sách khác hoặc giải thích dài. Chỉ trả về các dòng gợi ý hoặc câu từ chối chính xác như ở (2) hoặc 'Không có gợi ý nào phù hợp.'");
            sb.AppendLine("7) Nếu người yêu cầu món cụ thể (ví dụ: 'phở bò'), ưu tiên các nhà hàng có món đó gần vị trí người dùng.");
            sb.AppendLine("8) Tuân thủ chế độ ăn, dị ứng, sở thích có sẵn trong hồ sơ người dùng khi lọc gợi ý.");
            sb.AppendLine("9) Nếu cung cấp tối đa N gợi ý, giới hạn theo dữ liệu đầu vào; trong mọi trường hợp không hỏi lại người dùng.");

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

            sb.AppendLine("Danh sách nhà hàng đề xuất (tối đa 100):");
            int i = 1;
            foreach (var r in restaurants.Take(100))
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

            sb.AppendLine("Yêu cầu cuối: luôn trả về danh sách gợi ý món ăn và nhà hàng cụ thể theo định dạng được chỉ định ở trên; tuyệt đối KHÔNG hỏi lại người dùng; nếu ngoài phạm vi, trả lời chính xác: 'Xin lỗi, tôi chỉ hỗ trợ các câu hỏi về ăn uống và nhà hàng trong hệ thống AnGiDay.'");

            var recentPairs = recentMessagesSnapshot?.ToList() ?? new List<(string, string)>();

            return (sb.ToString(), recentPairs);
        }
    }
}
