using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class NotificationUser
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime? ReadAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    [JsonIgnore]
    public virtual Notification Notification { get; set; } = null!;
    [JsonIgnore]
    public virtual User User { get; set; } = null!;
}
