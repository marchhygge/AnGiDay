using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class RestaurantTag
{
    public int RestaurantId { get; set; }

    public int TagId { get; set; }

    public bool? IsDeleted { get; set; }
    [JsonIgnore]
    public virtual Restaurant Restaurant { get; set; } = null!;
    [JsonIgnore]
    public virtual Tag Tag { get; set; } = null!;
}
