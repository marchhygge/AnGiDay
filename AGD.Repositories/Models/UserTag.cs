using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class UserTag
{
    public int UserId { get; set; }

    public int TagId { get; set; }

    public bool? IsDeleted { get; set; }
    [JsonIgnore]
    public virtual Tag Tag { get; set; } = null!;
    [JsonIgnore]
    public virtual User User { get; set; } = null!;
}
