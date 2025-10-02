using System.Text.Json;
using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class Invoice
{
    public int Id { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public int? UserId { get; set; }

    public int? SubscriptionId { get; set; }

    public long TotalAmountCents { get; set; }

    public long? TaxAmountCents { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime? IssuedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public string PdfUrl { get; set; } = string.Empty;

    public JsonDocument? Metadata { get; set; } = JsonDocument.Parse("{}");

    public virtual Subscription? Subscription { get; set; }
    [JsonIgnore]
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User? User { get; set; }
}