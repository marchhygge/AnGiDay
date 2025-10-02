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

    public int? Rating { get; set; }
    [JsonIgnore]
    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    [JsonIgnore]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    [JsonIgnore]
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    [JsonIgnore]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    [JsonIgnore]
    public virtual ICollection<PostPerformance> PostPerformances { get; set; } = new List<PostPerformance>();

    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    [JsonIgnore]
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual Restaurant? Restaurant { get; set; }

    public virtual SignatureFood? SignatureFood { get; set; }
    
    public virtual User User { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<UserPostInteraction> UserPostInteractions { get; set; } = new List<UserPostInteraction>();
}
