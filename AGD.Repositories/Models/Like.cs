using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class Like
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    [JsonIgnore]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    [JsonIgnore]
    public virtual Post Post { get; set; } = null!;
    [JsonIgnore]
    public virtual User User { get; set; } = null!;
}
