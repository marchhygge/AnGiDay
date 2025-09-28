using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class Post
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? RestaurantId { get; set; }

    public string Type { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public int? SignatureFoodId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    [JsonIgnore]
    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    [JsonIgnore]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    
    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    [JsonIgnore]
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual Restaurant? Restaurant { get; set; }

    public virtual SignatureFood? SignatureFood { get; set; }
    
    public virtual User User { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<UserPostInteraction> UserPostInteractions { get; set; } = new List<UserPostInteraction>();
}
