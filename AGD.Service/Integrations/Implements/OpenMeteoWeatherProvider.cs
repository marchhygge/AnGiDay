using AGD.Service.Integrations.Interfaces;
using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace AGD.Service.Integrations.Implements
{
    public class OpenMeteoWeatherProvider : IWeatherProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IDistributedCache _cache;
        private readonly ILogger<OpenMeteoWeatherProvider> _logger;
        public OpenMeteoWeatherProvider(HttpClient httpClient, IDistributedCache cache, ILogger<OpenMeteoWeatherProvider> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        public async Task<WeatherInfo?> GetCurrentAsync(double lat, double lon, int cacheMinutes, CancellationToken ct = default)
        {
            var key = $"wx:{Math.Round(lat, 3)}:{Math.Round(lon, 3)}";
            try
            {
                var cached = await _cache.GetStringAsync(key, ct);
                if (!string.IsNullOrEmpty(cached))
                {
                    return JsonSerializer.Deserialize<WeatherInfo>(cached);
                }

                var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";
                using var response = await _httpClient.GetAsync(url, ct);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("OpenMeteo returned {StatusCode}", response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("current_weather", out var current)) return null;

                var temp = current.GetProperty("temperature").GetDouble();
                double precip = 0;
                var condition = temp > 30 ? "hot" : (temp < 18 ? "cool" : "mild");

                var info = new WeatherInfo(temp, precip, condition);
                var serialized = JsonSerializer.Serialize(info);
                await _cache.SetStringAsync(key, serialized, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Math.Max(1, cacheMinutes))
                }, ct);

                return info;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Weather provider error for {lat},{lon}", lat, lon);
                return null;
            }
        }
    }
}
