using System.Net.Http.Json;
using System.Text.Json;

namespace AGD.Service.Integrations
{
    public record OllamaMessage(string Role, string Content);
    public class OllamaEmbeddingClient
    {
        private readonly HttpClient _httpClient;
        public OllamaEmbeddingClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<float[]> EmbedAsync(string model, string text, CancellationToken ct = default)
        {
            var payload = new             
            {
                Model = model,
                Text = text
            };

            using var response = await _httpClient.PostAsJsonAsync("/api/embeddings", payload, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            using var document = JsonDocument.Parse(json);
            return document.RootElement.GetProperty("embedding").EnumerateArray().Select(e => e.GetSingle()).ToArray();
        }
    }
}
