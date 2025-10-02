using AGD.Repositories.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AGD.Repositories.Models
{
    public partial class Transaction
    {
        public int Id { get; set; }

        public Guid? Uuid { get; set; }

        public int? UserId { get; set; }

        public int? RestaurantId { get; set; }

        public int? SubscriptionId { get; set; }

        public int? InvoiceId { get; set; }

        public int? PaymentMethodId { get; set; }

        public PaymentProvider Provider { get; set; }

        public PaymentStatus Status { get; set; }

        public string ProviderTransactionId { get; set; } = string.Empty;

        public string IdempotencyKey { get; set; } = string.Empty;

        public long AmountCents { get; set; }

        public string Currency { get; set; } = "VND";

        public string Description { get; set; } = string.Empty;

        public string FailureReason { get; set; } = string.Empty;

        public long? RefundedAmountCents { get; set; }

        public bool? IsSettled { get; set; }

        public DateTime? SettledAt { get; set; }

        public JsonDocument? Metadata { get; set; } = JsonDocument.Parse("{}");

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        [JsonIgnore]
        public virtual ICollection<FinancialLedger> FinancialLedgers { get; set; } = new List<FinancialLedger>();
        
        public virtual Invoice? Invoice { get; set; }
        
        public virtual PaymentMethod? PaymentMethod { get; set; }

        public virtual Restaurant? Restaurant { get; set; }

        public virtual Subscription? Subscription { get; set; }

        public virtual User? User { get; set; }
    }
}
