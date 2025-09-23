using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class Conversation
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    [JsonIgnore]
    public virtual User User { get; set; } = null!;
}
