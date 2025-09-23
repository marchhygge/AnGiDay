using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class PostTag
{
    public int PostId { get; set; }

    public int TagId { get; set; }

    public bool IsDeleted { get; set; } = false;
    [JsonIgnore]
    public virtual Post Post { get; set; } = null!;
    [JsonIgnore]
    public virtual Tag Tag { get; set; } = null!;
}
