using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class HealthProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? DietType { get; set; }

    public string? HealthGoals { get; set; }

    public string? Allergies { get; set; }

    public string? Notes { get; set; }

    public bool IsDeleted { get; set; } = false;
    [JsonIgnore]
    public virtual User User { get; set; } = null!;
}
