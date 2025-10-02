namespace AGD.Repositories.Models;

public partial class PlanDoc
{
    public int Id { get; set; }

    public int? PlanId { get; set; }

    public string KeyName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; }

    public virtual Plan? Plan { get; set; }
}
