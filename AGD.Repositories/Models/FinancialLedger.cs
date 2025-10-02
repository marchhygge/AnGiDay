using AGD.Repositories.Enums;
using System.Text.Json;

namespace AGD.Repositories.Models;

public partial class FinancialLedger
{
    public int Id { get; set; }

    public int? TransactionId { get; set; }

    public LedgerEntryType EntryType { get; set; }

    public string Account { get; set; } = string.Empty;

    public long AmountCents { get; set; }

    public string Currency { get; set; } = string.Empty;

    public JsonDocument? Metadata { get; set; } = JsonDocument.Parse("{}");

    public DateTime? CreatedAt { get; set; }

    public virtual Transaction? Transaction { get; set; }
}