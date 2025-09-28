using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;
[Table("user_post_interactions")]
public partial class UserPostInteraction
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int PostId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsDeleted { get; set; }
    [JsonIgnore]
    public virtual Post Post { get; set; } = null!;
    [JsonIgnore]
    public virtual User User { get; set; } = null!;
}
