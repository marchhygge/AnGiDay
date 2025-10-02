using System.Text.Json;

namespace AGD.Repositories.Models
{
    public partial class WebhookEvent
    {
        public int Id { get; set; }

        public string EventType { get; set; } = string.Empty;

        public string ProviderEventId { get; set; } = string.Empty;

        public JsonDocument? RawPayload { get; set; }

        public DateTime? ReceivedAt { get; set; }

        public bool? Processed { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public JsonDocument? ProcessingResult { get; set; } = JsonDocument.Parse("{}");
    }
}
