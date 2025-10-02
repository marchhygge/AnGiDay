using System.Text.Json;
using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class PaymentMethod
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string ProviderPmId { get; set; } = string.Empty;

    public string Last4 { get; set; } = string.Empty;

    public string CardBrand { get; set; } = string.Empty;

    public short? ExpMonth { get; set; }

    public short? ExpYear { get; set; }

    public bool? IsDefault { get; set; }

    public JsonDocument? Metadata { get; set; } = JsonDocument.Parse("{}");

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }
    [JsonIgnore]
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User? User { get; set; }
}
