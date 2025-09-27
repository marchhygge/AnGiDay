using AGD.DAL.Basic;
using AGD.Repositories.DBContext;
using AGD.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace AGD.Repositories.Repositories
{
    public class MessageRepository : GenericRepository<Message>
    {
        public MessageRepository(AnGiDayContext context) : base(context)
        {
        }

        public async Task<Message> AddAsync(Message message, CancellationToken ct = default)
        {
            _context.Messages.Add(message);
            await SaveChangesAsync(ct);
            return message;
        }

        public async Task<List<Message>> GetRecentMessagesAsync(int conversationId, int limit, CancellationToken ct = default)
        {
            if (limit <= 0 || limit > 200) limit = 50;
            return await _context.Messages
                .AsNoTracking()
                .Where(m => m.ConversationId == conversationId && m.IsDeleted == false)
                .OrderByDescending(m => m.CreatedAt)
                .Take(limit)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<List<Message>> GetMessagesAsync(int conversationId, int limit, CancellationToken ct = default)
        {
            return await GetRecentMessagesAsync(conversationId, limit, ct);
        }
    }
}