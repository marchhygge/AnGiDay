using System.Text.Json;
using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class OwnerAnalytic
{
    public int Id { get; set; }

    public int RestaurantId { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public int? Views { get; set; }

    public int? Saves { get; set; }

    public int? Clicks { get; set; }

    public JsonDocument? Visitors { get; set; } = JsonDocument.Parse("[]");

    public DateTime? GeneratedAt { get; set; }
    [JsonIgnore]
    public virtual Restaurant Restaurant { get; set; } = null!;

}
