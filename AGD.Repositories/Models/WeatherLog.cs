using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class WeatherLog
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string WeatherData { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    [JsonIgnore]
    public virtual User User { get; set; } = null!;
}
