using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class Report
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    [JsonIgnore]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Post Post { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
