using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;
    [JsonIgnore]
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
