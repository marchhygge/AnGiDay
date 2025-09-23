using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class Comment
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    public int? ParentId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    public virtual ICollection<Comment> InverseParent { get; set; } = new List<Comment>();
    [JsonIgnore]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Comment? Parent { get; set; }

    public virtual Post Post { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
