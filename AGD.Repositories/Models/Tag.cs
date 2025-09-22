using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int CategoryId { get; set; }

    public string Description { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;

    public virtual Category Category { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    [JsonIgnore]
    public virtual ICollection<RestaurantTag> RestaurantTags { get; set; } = new List<RestaurantTag>();
    [JsonIgnore]
    public virtual ICollection<UserTag> UserTags { get; set; } = new List<UserTag>();
}
