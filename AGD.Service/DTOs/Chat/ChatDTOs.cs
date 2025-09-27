namespace AGD.Service.DTOs.Chat
{
    public record CreateConversationRequest(string? FirstMessage);
    public record ConversationDTO(int Id, string? Title);
    public record ChatRequestDTO(int ConversationId, string Message);
    public record MessageDTO(int Id, string Role, string Content, DateTime? CreatedAt);
    public record ChatResponseDTO(int ConversationId, MessageDTO UserMessage, MessageDTO AssistantMessage);
    public record ConversationListItemDTO(int Id, string? Title, DateTime? UpdatedAt);
}
