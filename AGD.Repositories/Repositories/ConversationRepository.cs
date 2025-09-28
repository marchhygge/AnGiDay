using AGD.DAL.Basic;
using AGD.Repositories.DBContext;
using AGD.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace AGD.Repositories.Repositories
{
    public class ConversationRepository : GenericRepository<Conversation>
    {
        public ConversationRepository(AnGiDayContext ctx) : base(ctx)
        {
        }
        public async Task<Conversation> AddAsync(Conversation conv, CancellationToken ct = default)
        {
            _context.Conversations.Add(conv);
            await _context.SaveChangesAsync(ct);
            return conv;
        }

        public async Task<Conversation?> GetByIdForUserAsync(int conversationId, int userId, CancellationToken ct = default)
        {
            return await _context.Conversations
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId && c.IsDeleted == false, ct);
        }

        public async Task MarkDeletedAsync(int conversationId, CancellationToken ct = default)
        {
            var conv = await _context.Conversations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == conversationId, ct);
            if (conv == null) return;
            conv.IsDeleted = true;
            await SaveChangesAsync(ct);
        }

        public async Task UpdateEndedAtAsync(int conversationId, CancellationToken ct = default)
        {
            var conv = await _context.Conversations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == conversationId, ct);
            if (conv == null) return;
            conv.EndedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await SaveChangesAsync(ct);
        }

        public async Task<List<Conversation>> ListForUserAsync(int userId, int page, int pageSize, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;
            return await _context.Conversations
                .AsNoTracking()
                .Where(c => c.UserId == userId && c.IsDeleted == false)
                .OrderByDescending(c => c.EndedAt ?? c.StartedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }
    }
}