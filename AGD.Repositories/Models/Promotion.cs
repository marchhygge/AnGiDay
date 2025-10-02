namespace AGD.Repositories.Models;

public partial class Promotion
{
    public int Id { get; set; }

    public int RestaurantId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal? DiscountPercent { get; set; }

    public decimal? DiscountAmount { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public int? UsageLimit { get; set; }

    public int? TimesUsed { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Restaurant Restaurant { get; set; } = null!;
}
