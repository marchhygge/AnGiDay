using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class UserRestaurantInteraction
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? RestaurantId { get; set; }

    public DateTime? LastInteractionAt { get; set; }

    public int? Rating { get; set; }

    public bool? Bookmarked { get; set; }

    public int? VisitCount { get; set; }

    public int? FavoriteFoodId { get; set; }

    public bool IsDeleted { get; set; } = false;
    [JsonIgnore]
    public virtual SignatureFood? FavoriteFood { get; set; }
    [JsonIgnore]    
    public virtual Restaurant? Restaurant { get; set; }
    [JsonIgnore]
    public virtual User User { get; set; } = null!;
}
