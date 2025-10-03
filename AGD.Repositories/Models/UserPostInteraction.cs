using System.Text.Json;
using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class UserPostInteraction
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int PostId { get; set; }

    public int? RestaurantId { get; set; }

    public string InteractionType { get; set; } = null!;

    public JsonDocument? Detail { get; set; } = JsonDocument.Parse("{}");

    public int? PlanId { get; set; }

    public int? SubscriptionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsDeleted { get; set; }
    [JsonIgnore]
    public virtual Post Post { get; set; } = null!;
    [JsonIgnore]
    public virtual Restaurant? Restaurant { get; set; }
    [JsonIgnore]
    public virtual User User { get; set; } = null!;
}
