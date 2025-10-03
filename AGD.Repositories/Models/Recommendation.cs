using System.Text.Json.Serialization;

namespace AGD.Repositories.Models
{
    public partial class Recommendation
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int RestaurantId { get; set; }

        public double Score { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual Restaurant Restaurant { get; set; } = null!;
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
    }
}
