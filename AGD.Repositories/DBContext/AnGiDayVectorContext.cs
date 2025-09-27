using AGD.Repositories.VectorModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AGD.Repositories.DBContext;

public partial class AnGiDayVectorContext : DbContext
{
    public AnGiDayVectorContext()
    {
    }

    public AnGiDayVectorContext(DbContextOptions<AnGiDayVectorContext> options)
        : base(options)
    {
    }

    public virtual DbSet<RestaurantEmbedding> RestaurantEmbeddings { get; set; }

    public virtual DbSet<SignatureFoodEmbedding> SignatureFoodEmbeddings { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseNpgsql("Persist Security Info=True;Password=123456;Username=postgres;Database=AnGiDayVector;Port=5433;Host=localhost");

    public static string? GetConnectionString(string connectionStringName)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = config.GetConnectionString(connectionStringName);
        return connectionString;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<RestaurantEmbedding>(entity =>
        {
            entity.HasKey(e => e.RestaurantId).HasName("restaurant_embeddings_pkey");

            entity.ToTable("restaurant_embeddings");

            entity.Property(e => e.RestaurantId)
                .ValueGeneratedNever()
                .HasColumnName("restaurant_id");
            entity.Property(e => e.Embedding).HasColumnType("vector(768)").HasColumnName("embedding");
            entity.Property(e => e.ModelName).HasColumnName("model_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<SignatureFoodEmbedding>(entity =>
        {
            entity.HasKey(e => e.SignatureFoodId).HasName("signature_food_embeddings_pkey");

            entity.ToTable("signature_food_embeddings");

            entity.Property(e => e.SignatureFoodId)
                .ValueGeneratedNever()
                .HasColumnName("signature_food_id");
            entity.Property(e => e.Embedding).HasColumnType("vector(768)").HasColumnName("embedding");
            entity.Property(e => e.ModelName).HasColumnName("model_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}