using System.Net.Http.Json;
using System.Text.Json;

namespace AGD.Service.Integrations
{
    public class OllamaClient
    {
        private readonly HttpClient _httpClient;
        public OllamaClient(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<string> ChatAsync(List<OllamaMessage> messages, string model, bool stream, CancellationToken ct)
        {
            var payload = new
            { 
                Model = model, 
                Messages = messages.Select(m => new { role = m.Role, content = m.Content }), 
                Stream = stream 
            };
            using var response = await _httpClient.PostAsJsonAsync("/api/chat", payload, ct);
            response.EnsureSuccessStatusCode();

            if (!stream)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var c))
                    return c.GetString() ?? "";
                return "";
            }

            var sb = new System.Text.StringBuilder();
            using var sr = new StreamReader(await response.Content.ReadAsStreamAsync(ct));
            while (!sr.EndOfStream)
            {
                var line = await sr.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;
                using var node = JsonDocument.Parse(line);
                if (node.RootElement.TryGetProperty("message", out var msg) &&
                    msg.TryGetProperty("content", out var c))
                {
                    var piece = c.GetString();
                    if (piece != null) sb.Append(piece);
                }
            }
            return sb.ToString();
        }
    }
}
