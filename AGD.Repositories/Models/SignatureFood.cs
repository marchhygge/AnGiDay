using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class SignatureFood
{
    public int Id { get; set; }

    public int RestaurantId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal? ReferencePrice { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;
    [JsonIgnore]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    [JsonIgnore]
    public virtual Restaurant Restaurant { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<UserRestaurantInteraction> UserRestaurantInteractions { get; set; } = new List<UserRestaurantInteraction>();
}
