using System.Text.Json;
using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class Notification
{
    public int Id { get; set; }

    public string Content { get; set; } = null!;

    public int? PostId { get; set; }

    public int? CommentId { get; set; }

    public int? LikeId { get; set; }

    public int? ReportId { get; set; }

    public JsonDocument? ExtraData { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    public NotificationType Type { get; set; }

    public virtual Comment? Comment { get; set; }

    public virtual Like? Like { get; set; }
    [JsonIgnore]
    public virtual ICollection<NotificationUser> NotificationUsers { get; set; } = new List<NotificationUser>();

    public virtual Post? Post { get; set; }

    public virtual Report? Report { get; set; }
}
