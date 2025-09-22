using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class RestaurantBranch
{
    public int Id { get; set; }

    public int RestaurantId { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string Address { get; set; } = null!;

    public bool? IsMain { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    [JsonIgnore]
    public virtual Restaurant Restaurant { get; set; } = null!;
}
