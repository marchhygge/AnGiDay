using System.Text.Json;
using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class Plan
{
    public int Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string RoleScope { get; set; } = string.Empty;

    public int? MonthlyPostLimit { get; set; }

    public int? BookmarkLimit { get; set; }

    public string AiChatLevel { get; set; } = string.Empty;

    public bool? Personalization { get; set; }

    public bool? AdvancedFilters { get; set; }

    public int? PromotedPriority { get; set; }

    public JsonDocument? FeatureFlags { get; set; } = JsonDocument.Parse("{}");

    public decimal? DiscountPercent { get; set; }

    public long? DiscountAmountCents { get; set; }

    public DateTime? DiscountValidFrom { get; set; }

    public DateTime? DiscountValidTo { get; set; }

    public JsonDocument? DiscountConditions { get; set; }

    public DateTime? CreatedAt { get; set; }
    [JsonIgnore]
    public virtual ICollection<PlanDoc> PlanDocs { get; set; } = new List<PlanDoc>();
    [JsonIgnore]
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
