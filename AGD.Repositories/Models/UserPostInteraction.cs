namespace AGD.Repositories.Models;

public partial class UserPostInteraction
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int PostId { get; set; }

    public int? RestaurantId { get; set; }

    public string InteractionType { get; set; } = null!;

    public string? Detail { get; set; }

    public int? PlanId { get; set; }

    public int? SubscriptionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Post Post { get; set; } = null!;

    public virtual Restaurant? Restaurant { get; set; }

    public virtual User User { get; set; } = null!;
}
