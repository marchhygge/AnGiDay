using AGD.Service.DTOs.Chat;

namespace AGD.Service.Services.Interfaces
{
    public interface IChatService
    {
        Task<ConversationDTO> CreateConversationAsync(int userId, string? firstMessage, CancellationToken ct);
        Task<ChatResponseDTO> SendMessageAsync(int userId, int conversationId, string userMessage, CancellationToken ct);
        Task<IReadOnlyList<ConversationListItemDTO>> ListConversationsAsync(int userId, int page, int pageSize, CancellationToken ct);
        Task<IReadOnlyList<MessageDTO>> GetMessagesAsync(int userId, int conversationId, int limit, CancellationToken ct);
        Task DeleteConversationAsync(int userId, int conversationId, CancellationToken ct);
    }
}
