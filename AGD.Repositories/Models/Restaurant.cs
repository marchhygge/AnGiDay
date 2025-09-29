using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class Restaurant
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public int OwnerId { get; set; }

    public string Description { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public string Status { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    public decimal? AvgRating { get; set; }

    public int? RatingCount { get; set; }
    [JsonIgnore]
    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual User Owner { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    [JsonIgnore]
    public virtual ICollection<RestaurantBranch> RestaurantBranches { get; set; } = new List<RestaurantBranch>();
    [JsonIgnore]
    public virtual ICollection<RestaurantTag> RestaurantTags { get; set; } = new List<RestaurantTag>();
    [JsonIgnore]
    public virtual ICollection<SignatureFood> SignatureFoods { get; set; } = new List<SignatureFood>();
    [JsonIgnore]
    public virtual ICollection<UserPostInteraction> UserPostInteractions { get; set; } = new List<UserPostInteraction>();
    [JsonIgnore]
    public virtual ICollection<UserRestaurantInteraction> UserRestaurantInteractions { get; set; } = new List<UserRestaurantInteraction>();
}
