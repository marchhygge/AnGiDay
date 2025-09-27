namespace AGD.Service.Integrations.Interfaces
{
    public record WeatherInfo(double TemperatureCelsius, double Precipitation, string ConditionSummary);
    public interface IWeatherProvider
    {
        Task<WeatherInfo?> GetCurrentAsync(double lat, double lon, int cacheMinutes, CancellationToken ct = default);
    }
}
