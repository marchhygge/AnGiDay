using System.Text.Json;

namespace AGD.Repositories.Models;

public partial class PostPerformance
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public int? Views { get; set; }

    public int? Saves { get; set; }

    public int? Clicks { get; set; }

    public JsonDocument? Metrics { get; set; } = JsonDocument.Parse("{}");

    public DateTime? GeneratedAt { get; set; }

    public virtual Post Post { get; set; } = null!;
}
