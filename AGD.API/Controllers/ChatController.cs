using AGD.Service.DTOs.Chat;
using AGD.Service.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AGD.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;
        public ChatController(IServicesProvider servicesProvider) => _servicesProvider = servicesProvider;

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                      User.FindFirstValue("sub") ?? throw new UnauthorizedAccessException());

        [HttpPost("conversation")]
        public async Task<IActionResult> Create([FromBody] CreateConversationRequest req, CancellationToken ct)
            => Ok(await _servicesProvider.ChatService.CreateConversationAsync(CurrentUserId, req.FirstMessage, ct));

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] ChatRequestDTO req, CancellationToken ct)
            => Ok(await _servicesProvider.ChatService.SendMessageAsync(CurrentUserId, req.ConversationId, req.Message, ct));

        [HttpGet("conversations")]
        public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
            => Ok(await _servicesProvider.ChatService.ListConversationsAsync(CurrentUserId, page, pageSize, ct));

        [HttpGet("conversation/{conversationId}/messages")]
        public async Task<IActionResult> Messages([FromRoute] int conversationId, [FromQuery] int limit = 50, CancellationToken ct = default)
            => Ok(await _servicesProvider.ChatService.GetMessagesAsync(CurrentUserId, conversationId, limit, ct));

        [HttpDelete("conversation/{conversationId}")]
        public async Task<IActionResult> Delete([FromRoute] int conversationId, CancellationToken ct)
        {
            await _servicesProvider.ChatService.DeleteConversationAsync(CurrentUserId, conversationId, ct);
            return NoContent();
        }
    }
}