using System.Text.Json;
using System.Text.Json.Serialization;

namespace AGD.Repositories.Models
{
    public partial class Subscription
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int PlanId { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime? EndAt { get; set; }

        public bool IsActive { get; set; }

        public JsonDocument? Metadata { get; set; } = JsonDocument.Parse("{}");

        public DateTime? CreatedAt { get; set; }
        [JsonIgnore]
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

        public virtual Plan Plan { get; set; } = null!;
        [JsonIgnore]
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        public virtual User User { get; set; } = null!;
    }
}
