using AGD.Repositories.DBContext;
using AGD.Repositories.Helpers;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace AGD.Repositories.Repositories
{
    public interface IUnitOfWork : IDisposable
    {       
        JwtHelper JwtHelper { get; }
        Task<int> SaveChangesAsync(CancellationToken ct);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
        Task CommitTransactionAsync(CancellationToken ct = default);
        Task RollbackTransactionAsync(CancellationToken ct = default);
        // khi cần gom nhiều thao tác vào 1 transaction thì dùng hàm này
        Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default);
        RestaurantRepository RestaurantRepository { get; }
        UserRepository UserRepository { get; }
        BookmarkRepository BookmarkRepository { get; }
        EmbeddingRepository EmbeddingRepository { get; }
        ConversationRepository ConversationRepository { get; }
        MessageRepository MessageRepository { get; }
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly AnGiDayContext _context;
        private readonly JwtHelper _jwtHelper;
        private readonly string _vectorConnection;
        private IDbContextTransaction? _transaction;
        private bool _disposed;
        private RestaurantRepository? _restaurantRepository;
        private UserRepository? _userRepository;
        private BookmarkRepository? _bookmarkRepository;
        private EmbeddingRepository? _embeddingRepository;
        private ConversationRepository? _conversationRepository;
        private MessageRepository? _messageRepository;

        public UnitOfWork(JwtHelper jwtHelper, AnGiDayContext context, IConfiguration configuration)
        {
            _jwtHelper = jwtHelper;
            _context = context;
            _vectorConnection = configuration.GetConnectionString("EmbeddingConnection") ?? throw new ArgumentNullException("Vector database connection string is not configured.");
        }

        public RestaurantRepository RestaurantRepository => _restaurantRepository ??= new RestaurantRepository(_context);
        public UserRepository UserRepository => _userRepository ??= new UserRepository(_context);
        public BookmarkRepository BookmarkRepository => _bookmarkRepository ??= new BookmarkRepository(_context);
        public EmbeddingRepository EmbeddingRepository => _embeddingRepository ??= new EmbeddingRepository(_vectorConnection);
        public ConversationRepository ConversationRepository => _conversationRepository ??= new ConversationRepository(_context);
        public MessageRepository MessageRepository => _messageRepository ??= new MessageRepository(_context);
        public JwtHelper JwtHelper => _jwtHelper;

        void IDisposable.Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            await _context.DisposeAsync();
            GC.SuppressFinalize(this);
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct)
        {
            return await _context.SaveChangesAsync(ct); // thay đổi code cũ 1 tí để EF core tự quản lý transaction cho 1 lần save changes
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction != null) return _transaction;
            _transaction = await _context.Database.BeginTransactionAsync(ct);
            return _transaction;
        }

        public async Task CommitTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction == null) return;
            await _context.SaveChangesAsync(ct);
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction == null) return;
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        // đổi code cũ của save changes xuống này để tối ưu
        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default)
        {
            await using (var transaction = await _context.Database.BeginTransactionAsync(ct))
            {
                try
                {
                    await action(ct); //này là thực hiện nhiều repo nè
                    await _context.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            }
        }
    }
}
