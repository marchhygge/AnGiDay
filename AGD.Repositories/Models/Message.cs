using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class Message
{
    public int Id { get; set; }

    public int ConversationId { get; set; }

    public string Sender { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    [JsonIgnore]
    public virtual Conversation Conversation { get; set; } = null!;
}
