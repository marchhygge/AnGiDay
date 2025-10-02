using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class Bookmark
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? PostId { get; set; }

    public int? RestaurantId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    public virtual Post? Post { get; set; }

    public virtual Restaurant? Restaurant { get; set; }
    [JsonIgnore]
    public virtual User User { get; set; } = null!;
}
