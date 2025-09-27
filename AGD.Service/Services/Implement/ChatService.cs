using AGD.Repositories.Models;
using AGD.Repositories.Repositories;
using AGD.Service.DTOs.Chat;
using AGD.Service.Integrations;
using AGD.Service.Services.Interfaces;
using AGD.Service.Services.Retrieval;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace AGD.Service.Services.Implement
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRestaurantRetrieval _retrieval;
        private readonly VectorRetrievalService _vectorRetrieval;
        private readonly ContextBuilder _contextBuilder;
        private readonly OllamaClient _ollama;
        private readonly IConfiguration _cfg;

        public ChatService(
            IUnitOfWork unitOfWork,
            IRestaurantRetrieval retrieval,
            VectorRetrievalService vectorRetrieval,
            ContextBuilder contextBuilder,
            OllamaClient ollama,
            IConfiguration cfg)
        {
            _unitOfWork = unitOfWork;
            _retrieval = retrieval;
            _vectorRetrieval = vectorRetrieval;
            _contextBuilder = contextBuilder;
            _ollama = ollama;
            _cfg = cfg;
        }

        public async Task<ConversationDTO> CreateConversationAsync(int userId, string? firstMessage, CancellationToken ct)
        {
            var conv = new Conversation
            {
                UserId = userId,
                StartedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var created = await _unitOfWork.ConversationRepository.AddAsync(conv, ct);

            if (!string.IsNullOrWhiteSpace(firstMessage))
            {
                await SendMessageAsync(userId, created.Id, firstMessage, ct);
            }

            return new ConversationDTO(created.Id, created.Title);
        }

        public async Task<ChatResponseDTO> SendMessageAsync(int userId, int conversationId, string userMessage, CancellationToken ct)
        {
            var conv = await _unitOfWork.ConversationRepository.GetByIdForUserAsync(conversationId, userId, ct)
                       ?? throw new UnauthorizedAccessException("Conversation not found or not owned by user");

            var userMsg = new Message
            {
                ConversationId = conversationId,
                Sender = "user",
                Content = userMessage,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            userMsg = await _unitOfWork.MessageRepository.AddAsync(userMsg, ct);

            int topK = int.Parse(_cfg["Ollama:TopK"] ?? "5");
            List<dynamic> restaurants;
            List<dynamic> foods;
            try
            {
                var (restList, foodList) = await _retrieval.FindCandidatesAsync(userId, userMessage, null, null, topK, ct);
                restaurants = restList.Select(r => (dynamic)r).ToList();
                foods = foodList.Select(f => (dynamic)f).ToList();
            }
            catch
            {
                restaurants = new List<dynamic>();
                foods = new List<dynamic>();
            }

            var recentMessages = await _unitOfWork.MessageRepository.GetRecentMessagesAsync(conversationId, int.Parse(_cfg["Ollama:MaxContextMessages"] ?? "20"), ct);
            var recentPairs = recentMessages.Select(m => (role: m.Sender.Equals("AI", StringComparison.OrdinalIgnoreCase) ? "assistant" : "user", content: m.Content)).ToList();

            var (systemPrompt, recentForModel) = await _contextBuilder.BuildAsync(userId, conversationId, userMessage, restaurants, foods, recentPairs, ct);

            var model = _cfg["Ollama:Model"] ?? "mistral:7b";
            var messages = new List<OllamaMessage> { new OllamaMessage("system", systemPrompt) };
            messages.AddRange(recentForModel.Select(r => new OllamaMessage(r.role, r.content)));
            messages.Add(new OllamaMessage("user", userMessage));

            string assistantText;
            try
            {
                assistantText = await _ollama.ChatAsync(messages, model, stream: false, ct);
            }
            catch (Exception)
            {
                assistantText = "Xin lỗi, hiện tại hệ thống gợi ý đang tạm không hoạt động. Bạn thử lại sau.";
            }

            var aiMsg = new Message
            {
                ConversationId = conversationId,
                Sender = "AI",
                Content = assistantText,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                ModelName = model,
                Meta = JsonSerializer.Serialize(new { candidates = restaurants.Take(5).Select(r => r.Name) })
            };
            aiMsg = await _unitOfWork.MessageRepository.AddAsync(aiMsg, ct);

            // 8) update conversation ended_at (non-blocking)
            _ = _unitOfWork.ConversationRepository.UpdateEndedAtAsync(conversationId, ct);

            return new ChatResponseDTO(
                conversationId,
                new MessageDTO(userMsg.Id, "user", userMsg.Content, userMsg.CreatedAt),
                new MessageDTO(aiMsg.Id, "assistant", aiMsg.Content, aiMsg.CreatedAt)
            );
        }

        public async Task<IReadOnlyList<ConversationListItemDTO>> ListConversationsAsync(int userId, int page, int pageSize, CancellationToken ct)
        {
            var list = await _unitOfWork.ConversationRepository.ListForUserAsync(userId, page, pageSize, ct);
            return list.Select(c => new ConversationListItemDTO(c.Id, c.Title, c.EndedAt ?? c.StartedAt)).ToList();
        }

        public async Task<IReadOnlyList<MessageDTO>> GetMessagesAsync(int userId, int conversationId, int limit, CancellationToken ct)
        {
            var conv = await _unitOfWork.ConversationRepository.GetByIdForUserAsync(conversationId, userId, ct);
            if (conv == null) throw new UnauthorizedAccessException();

            var msgs = await _unitOfWork.MessageRepository.GetMessagesAsync(conversationId, limit, ct);
            return msgs.Select(m => new MessageDTO(m.Id, m.Sender.Equals("AI", StringComparison.OrdinalIgnoreCase) ? "assistant" : "user", m.Content, m.CreatedAt)).ToList();
        }

        public async Task DeleteConversationAsync(int userId, int conversationId, CancellationToken ct)
        {
            var conv = await _unitOfWork.ConversationRepository.GetByIdForUserAsync(conversationId, userId, ct);
            if (conv == null) return;
            await _unitOfWork.ConversationRepository.MarkDeletedAsync(conversationId, ct);
        }
    }
}