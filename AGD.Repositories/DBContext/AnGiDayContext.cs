using AGD.Repositories.Enums;
using AGD.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AGD.Repositories.DBContext;

public partial class AnGiDayContext : DbContext
{
    public AnGiDayContext()
    {
    }

    public AnGiDayContext(DbContextOptions<AnGiDayContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bookmark> Bookmarks { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Comment> Comments { get; set; }
    public virtual DbSet<Conversation> Conversations { get; set; }
    public virtual DbSet<HealthProfile> HealthProfiles { get; set; }
    public virtual DbSet<Like> Likes { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<NotificationUser> NotificationUsers { get; set; }
    public virtual DbSet<Post> Posts { get; set; }
    public virtual DbSet<PostTag> PostTags { get; set; }
    public virtual DbSet<Report> Reports { get; set; }
    public virtual DbSet<Restaurant> Restaurants { get; set; }
    public virtual DbSet<RestaurantBranch> RestaurantBranches { get; set; }
    public virtual DbSet<RestaurantTag> RestaurantTags { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<SignatureFood> SignatureFoods { get; set; }
    public virtual DbSet<Tag> Tags { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserLocation> UserLocations { get; set; }
    public virtual DbSet<UserPostInteraction> UserPostInteractions { get; set; }
    public virtual DbSet<UserRestaurantInteraction> UserRestaurantInteractions { get; set; }
    public virtual DbSet<UserTag> UserTags { get; set; }
    public virtual DbSet<WeatherLog> WeatherLogs { get; set; }

    public static string? GetConnectionString(string connectionStringName)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = config.GetConnectionString(connectionStringName);
        return connectionString;
    }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    => optionsBuilder
    //        .UseNpgsql(GetConnectionString("DefaultConnection"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<LedgerEntryType>("public", "ledger_entry_type")
            .HasPostgresEnum<NotificationType>("public", "notification_type")
            .HasPostgresEnum<PaymentProvider>("public", "payment_provider")
            .HasPostgresEnum<PaymentStatus>("public", "payment_status")
            .HasPostgresEnum<UserStatus>("public", "user_status")
            .HasPostgresExtension("pgcrypto"); ;

        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bookmarks_pkey");
            entity.ToTable("bookmarks");

            entity.HasIndex(e => e.PostId, "idx_bookmarks_post");
            entity.HasIndex(e => e.RestaurantId, "idx_bookmarks_restaurant");
            entity.HasIndex(e => e.UserId, "idx_bookmarks_user");

            entity.HasIndex(e => new { e.UserId, e.PostId })
                  .HasDatabaseName("ux_bookmarks_user_post")
                  .IsUnique()
                  .HasFilter("post_id IS NOT NULL");

            entity.HasIndex(e => new { e.UserId, e.RestaurantId })
                  .HasDatabaseName("ux_bookmarks_user_restaurant")
                  .IsUnique()
                  .HasFilter("restaurant_id IS NOT NULL");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).IsRequired().HasColumnName("user_id");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.RestaurantId).HasColumnName("restaurant_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired()
                .HasColumnName("is_deleted");

            entity.HasOne(d => d.Post).WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("bookmarks_post_id_fkey");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("bookmarks_restaurant_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("bookmarks_user_id_fkey");

            entity.ToTable(tb =>
            {
                tb.HasCheckConstraint(
                    "only_one_bookmark_target",
                    "(post_id IS NOT NULL)::int + (restaurant_id IS NOT NULL)::int = 1"
                );
            });
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");
            entity.ToTable("categories");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired()
                .HasColumnName("is_deleted");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("comments_pkey");
            entity.ToTable("comments");

            entity.HasIndex(e => e.ParentId, "idx_comments_parent");
            entity.HasIndex(e => e.PostId, "idx_comments_post");
            entity.HasIndex(e => e.UserId, "idx_comments_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PostId).IsRequired().HasColumnName("post_id");
            entity.Property(e => e.UserId).IsRequired().HasColumnName("user_id");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Content).IsRequired().HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired()
                .HasColumnName("is_deleted");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("comments_parent_id_fkey");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("comments_post_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("comments_user_id_fkey");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("conversations_pkey");

            entity.ToTable("conversations");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EndedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ended_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("started_at");
            entity.Property(e => e.Summary).HasColumnName("summary");
            entity.Property(e => e.SummaryTokenCount)
                .HasDefaultValue(0)
                .HasColumnName("summary_token_count");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("conversations_user_id_fkey");
        });

        modelBuilder.Entity<FinancialLedger>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("financial_ledger_pkey");

            entity.ToTable("financial_ledger");

            entity.HasIndex(e => e.TransactionId, "idx_ledger_txn");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(l => l.EntryType).HasColumnType("ledger_entry_type").HasColumnName("entry_type");
            entity.Property(e => e.Account)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("account");
            entity.Property(e => e.AmountCents).HasColumnName("amount_cents");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(8)
                .HasDefaultValueSql("'VND'::character varying")
                .HasColumnName("currency");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Transaction).WithMany(p => p.FinancialLedgers)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("financial_ledger_transaction_id_fkey");
        });

        modelBuilder.Entity<HealthProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("health_profiles_pkey");
            entity.ToTable("health_profiles");

            entity.HasIndex(e => e.UserId, "health_profiles_user_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).IsRequired().HasColumnName("user_id");
            entity.Property(e => e.DietType).HasMaxLength(50).HasColumnName("diet_type");
            entity.Property(e => e.HealthGoals).HasMaxLength(100).HasColumnName("health_goals");
            entity.Property(e => e.Allergies).HasColumnName("allergies");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired().HasColumnName("is_deleted");

            entity.HasOne(d => d.User).WithOne(p => p.HealthProfile)
                .HasForeignKey<HealthProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("health_profiles_user_id_fkey");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("invoices_pkey");

            entity.ToTable("invoices");

            entity.HasIndex(e => e.UserId, "idx_invoices_user");
            entity.HasIndex(e => e.InvoiceNumber, "invoices_invoice_number_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Currency)
                .HasMaxLength(8)
                .HasDefaultValueSql("'VND'::character varying")
                .HasColumnName("currency");
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(100)
                .HasColumnName("invoice_number");
            entity.Property(e => e.IssuedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("issued_at");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.PaidAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("paid_at");
            entity.Property(e => e.PdfUrl).HasColumnName("pdf_url");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'issued'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");
            entity.Property(e => e.TaxAmountCents)
                .HasDefaultValue(0L)
                .HasColumnName("tax_amount_cents");
            entity.Property(e => e.TotalAmountCents).HasColumnName("total_amount_cents");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Subscription).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("invoices_subscription_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("invoices_user_id_fkey");
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("likes_pkey");
            entity.ToTable("likes");

            entity.HasIndex(e => e.PostId, "idx_likes_post");
            entity.HasIndex(e => e.UserId, "idx_likes_user");
            entity.HasIndex(e => new { e.PostId, e.UserId }, "likes_post_id_user_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PostId).IsRequired().HasColumnName("post_id");
            entity.Property(e => e.UserId).IsRequired().HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired()
                .HasColumnName("is_deleted");

            entity.HasOne(d => d.Post).WithMany(p => p.Likes)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("likes_post_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Likes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("likes_user_id_fkey");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("messages_pkey");

            entity.ToTable("messages");

            entity.HasIndex(e => e.ConversationId, "idx_messages_conversation");

            entity.HasIndex(e => new { e.ConversationId, e.CreatedAt }, "idx_messages_conversation_not_deleted").HasFilter("(is_deleted = false)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnName("content");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Meta)
                .HasColumnType("jsonb")
                .HasColumnName("meta");
            entity.Property(e => e.ModelName).HasColumnName("model_name");
            entity.Property(e => e.Sender)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("sender");
            entity.Property(e => e.TokensIn).HasColumnName("tokens_in");
            entity.Property(e => e.TokensOut).HasColumnName("tokens_out");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("messages_conversation_id_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notifications_pkey");
            entity.ToTable("notifications");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).IsRequired().HasColumnName("content");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.LikeId).HasColumnName("like_id");
            entity.Property(e => e.ReportId).HasColumnName("report_id");

            entity.Property(e => e.Type)
                .IsRequired()
                .HasColumnType("notification_type")
                .HasColumnName("type");

            entity.Property(e => e.ExtraData)
                .HasColumnType("jsonb")
                .HasColumnName("extra_data");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired()
                .HasColumnName("is_deleted");

            entity.HasOne(d => d.Comment).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.CommentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notifications_comment_id_fkey");

            entity.HasOne(d => d.Like).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.LikeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notifications_like_id_fkey");

            entity.HasOne(d => d.Post).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notifications_post_id_fkey");

            entity.HasOne(d => d.Report).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ReportId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notifications_report_id_fkey");

            entity.ToTable(tb =>
            {
                tb.HasCheckConstraint(
                    "only_one_target",
                    "(post_id IS NOT NULL)::int + (comment_id IS NOT NULL)::int + (like_id IS NOT NULL)::int + (report_id IS NOT NULL)::int <= 1"
                );
            });
        });

        modelBuilder.Entity<NotificationUser>(entity =>
        {
            entity.HasKey(e => new { e.NotificationId, e.UserId }).HasName("notification_users_pkey");
            entity.ToTable("notification_users");

            entity.HasIndex(e => e.NotificationId, "idx_notification_users_notification");
            entity.HasIndex(e => new { e.UserId, e.IsRead }, "idx_notification_users_user");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.IsRead).HasDefaultValue(false).IsRequired().HasColumnName("is_read");
            entity.Property(e => e.ReadAt).HasColumnType("timestamp without time zone").HasColumnName("read_at");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired().HasColumnName("is_deleted");

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationUsers)
                .HasForeignKey(d => d.NotificationId)
                .HasConstraintName("notification_users_notification_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("notification_users_user_id_fkey");
        });

        modelBuilder.Entity<OwnerAnalytic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("owner_analytics_pkey");

            entity.ToTable("owner_analytics");

            entity.HasIndex(e => new { e.RestaurantId, e.PeriodStart }, "idx_owner_analytics_period");
            entity.HasIndex(e => new { e.RestaurantId, e.PeriodStart }, "idx_owner_analytics_restaurant_period");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Clicks)
                .HasDefaultValue(0)
                .HasColumnName("clicks");
            entity.Property(e => e.GeneratedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("generated_at");
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end");
            entity.Property(e => e.PeriodStart).HasColumnName("period_start");
            entity.Property(e => e.RestaurantId).HasColumnName("restaurant_id");
            entity.Property(e => e.Saves)
                .HasDefaultValue(0)
                .HasColumnName("saves");
            entity.Property(e => e.Views)
                .HasDefaultValue(0)
                .HasColumnName("views");
            entity.Property(e => e.Visitors)
                .HasDefaultValueSql("'[]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("visitors");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.OwnerAnalytics)
                .HasForeignKey(d => d.RestaurantId)
                .HasConstraintName("owner_analytics_restaurant_id_fkey");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_methods_pkey");

            entity.ToTable("payment_methods");

            entity.HasIndex(e => e.UserId, "idx_payment_methods_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CardBrand)
                .HasMaxLength(50)
                .HasColumnName("card_brand");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpMonth).HasColumnName("exp_month");
            entity.Property(e => e.ExpYear).HasColumnName("exp_year");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Last4)
                .HasMaxLength(4)
                .HasColumnName("last4");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.ProviderPmId)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("provider_pm_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.PaymentMethods)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("payment_methods_user_id_fkey");
        });

        modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("plans_pkey");

            entity.ToTable("plans");

            entity.HasIndex(e => e.Slug, "plans_slug_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdvancedFilters)
                .HasDefaultValue(false)
                .HasColumnName("advanced_filters");
            entity.Property(e => e.AiChatLevel)
                .HasMaxLength(20)
                .HasColumnName("ai_chat_level");
            entity.Property(e => e.BookmarkLimit).HasColumnName("bookmark_limit");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountAmountCents).HasColumnName("discount_amount_cents");
            entity.Property(e => e.DiscountConditions)
                .HasColumnType("jsonb")
                .HasColumnName("discount_conditions");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(5, 2)
                .HasColumnName("discount_percent");
            entity.Property(e => e.DiscountValidFrom)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("discount_valid_from");
            entity.Property(e => e.DiscountValidTo)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("discount_valid_to");
            entity.Property(e => e.FeatureFlags)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("feature_flags");
            entity.Property(e => e.MonthlyPostLimit).HasColumnName("monthly_post_limit");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Personalization)
                .HasDefaultValue(false)
                .HasColumnName("personalization");
            entity.Property(e => e.PromotedPriority)
                .HasDefaultValue(0)
                .HasColumnName("promoted_priority");
            entity.Property(e => e.RoleScope)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("role_scope");
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("slug");
        });

        modelBuilder.Entity<PlanDoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("plan_docs_pkey");

            entity.ToTable("plan_docs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.KeyName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("key_name");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");

            entity.HasOne(d => d.Plan).WithMany(p => p.PlanDocs)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("plan_docs_plan_id_fkey");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("posts_pkey");

            entity.ToTable("posts");

            entity.HasIndex(e => e.CreatedAt, "idx_posts_created_at");
            entity.HasIndex(e => e.IsDeleted, "idx_posts_is_deleted");
            entity.HasIndex(e => e.RestaurantId, "idx_posts_restaurant");
            entity.HasIndex(e => e.SignatureFoodId, "idx_posts_signature_food");
            entity.HasIndex(e => e.Type, "idx_posts_type");
            entity.HasIndex(e => e.UserId, "idx_posts_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("image_url");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.RestaurantId).HasColumnName("restaurant_id");
            entity.Property(e => e.SignatureFoodId).HasColumnName("signature_food_id");
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.Posts)
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("posts_restaurant_id_fkey");

            entity.HasOne(d => d.SignatureFood).WithMany(p => p.Posts)
                .HasForeignKey(d => d.SignatureFoodId)
                .HasConstraintName("posts_signature_food_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Posts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("posts_user_id_fkey");
        });

        modelBuilder.Entity<PostPerformance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_performance_pkey");

            entity.ToTable("post_performance");

            entity.HasIndex(e => e.PostId, "idx_post_perf_post");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Clicks)
                .HasDefaultValue(0)
                .HasColumnName("clicks");
            entity.Property(e => e.GeneratedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("generated_at");
            entity.Property(e => e.Metrics)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metrics");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.Saves)
                .HasDefaultValue(0)
                .HasColumnName("saves");
            entity.Property(e => e.Views)
                .HasDefaultValue(0)
                .HasColumnName("views");

            entity.HasOne(d => d.Post).WithMany(p => p.PostPerformances)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("post_performance_post_id_fkey");
        });

        modelBuilder.Entity<PostTag>(entity =>
        {
            entity.HasKey(e => new { e.PostId, e.TagId }).HasName("post_tags_pkey");
            entity.ToTable("post_tags");

            entity.HasIndex(e => e.PostId, "idx_post_tags_post");
            entity.HasIndex(e => e.TagId, "idx_post_tags_tag");

            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.TagId).HasColumnName("tag_id");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired().HasColumnName("is_deleted");

            entity.HasOne(d => d.Post).WithMany(p => p.PostTags)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("post_tags_post_id_fkey");

            entity.HasOne(d => d.Tag).WithMany(p => p.PostTags)
                .HasForeignKey(d => d.TagId)
                .HasConstraintName("post_tags_tag_id_fkey");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("promotions_pkey");

            entity.ToTable("promotions");

            entity.HasIndex(e => new { e.RestaurantId, e.IsActive }, "idx_promotions_restaurant");
            entity.HasIndex(e => new { e.RestaurantId, e.Code }, "ux_promotions_code_restaurant").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(12, 2)
                .HasColumnName("discount_amount");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(5, 2)
                .HasColumnName("discount_percent");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.RestaurantId).HasColumnName("restaurant_id");
            entity.Property(e => e.TimesUsed)
                .HasDefaultValue(0)
                .HasColumnName("times_used");
            entity.Property(e => e.UsageLimit).HasColumnName("usage_limit");
            entity.Property(e => e.ValidFrom)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("valid_from");
            entity.Property(e => e.ValidTo)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("valid_to");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.Promotions)
                .HasForeignKey(d => d.RestaurantId)
                .HasConstraintName("promotions_restaurant_id_fkey");
        });

        modelBuilder.Entity<Recommendation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("recommendation_pkey");

            entity.ToTable("recommendation");

            entity.HasIndex(e => new { e.RestaurantId, e.Score }, "idx_recommendation_restaurant_score").IsDescending(false, true);
            entity.HasIndex(e => new { e.UserId, e.Score }, "idx_recommendation_user_score").IsDescending(false, true);
            entity.HasIndex(e => new { e.UserId, e.RestaurantId }, "ux_recommendation_user_restaurant").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RestaurantId).HasColumnName("restaurant_id");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.Recommendations)
                .HasForeignKey(d => d.RestaurantId)
                .HasConstraintName("recommendation_restaurant_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Recommendations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("recommendation_user_id_fkey");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reports_pkey");
            entity.ToTable("reports");

            entity.HasIndex(e => e.PostId, "idx_reports_post");
            entity.HasIndex(e => e.Status, "idx_reports_status");
            entity.HasIndex(e => e.UserId, "idx_reports_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PostId).IsRequired().HasColumnName("post_id");
            entity.Property(e => e.UserId).IsRequired().HasColumnName("user_id");
            entity.Property(e => e.Reason).IsRequired().HasColumnName("reason");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValueSql("'pending'::character varying").HasColumnName("status");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("created_at");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired().HasColumnName("is_deleted");

            entity.HasOne(d => d.Post).WithMany(p => p.Reports)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("reports_post_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Reports)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("reports_user_id_fkey");
        });

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("restaurants_pkey");

            entity.ToTable("restaurants");

            entity.HasIndex(e => new { e.Latitude, e.Longitude }, "idx_restaurants_location");

            entity.HasIndex(e => e.Name, "idx_restaurants_name");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.AllowPromotion)
                .HasDefaultValue(true)
                .HasColumnName("allow_promotion");
            entity.Property(e => e.AvgRating)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("avg_rating");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("image_url");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(30)
                .HasColumnName("phone_number");
            entity.Property(e => e.PromotedPriority)
                .HasDefaultValue(0)
                .HasColumnName("promoted_priority");
            entity.Property(e => e.PromotedUntil)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("promoted_until");
            entity.Property(e => e.RatingCount)
                .HasDefaultValue(0)
                .HasColumnName("rating_count");
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Owner).WithMany(p => p.Restaurants)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("restaurants_owner_id_fkey");
        });

        modelBuilder.Entity<RestaurantBranch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("restaurant_branches_pkey");
            entity.ToTable("restaurant_branches");

            entity.HasIndex(e => e.RestaurantId, "idx_restaurant_branches_restaurant");
            entity.HasIndex(e => e.RestaurantId, "ux_restaurant_main_branch")
                .IsUnique()
                .HasFilter("(is_main = true)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RestaurantId).IsRequired().HasColumnName("restaurant_id");
            entity.Property(e => e.Latitude).IsRequired().HasColumnName("latitude");
            entity.Property(e => e.Longitude).IsRequired().HasColumnName("longitude");
            entity.Property(e => e.Address).IsRequired().HasMaxLength(255).HasColumnName("address");
            entity.Property(e => e.IsMain).HasDefaultValue(false).HasColumnName("is_main");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("created_at");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired().HasColumnName("is_deleted");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.RestaurantBranches)
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("restaurant_branches_restaurant_id_fkey");
        });

        modelBuilder.Entity<RestaurantTag>(entity =>
        {
            entity.HasKey(e => new { e.RestaurantId, e.TagId }).HasName("restaurant_tags_pkey");
            entity.ToTable("restaurant_tags");

            entity.HasIndex(e => e.RestaurantId, "idx_restaurant_tags_restaurant");
            entity.HasIndex(e => e.TagId, "idx_restaurant_tags_tag");

            entity.Property(e => e.RestaurantId).HasColumnName("restaurant_id");
            entity.Property(e => e.TagId).HasColumnName("tag_id");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).HasColumnName("is_deleted");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.RestaurantTags)
                .HasForeignKey(d => d.RestaurantId)
                .HasConstraintName("restaurant_tags_restaurant_id_fkey");

            entity.HasOne(d => d.Tag).WithMany(p => p.RestaurantTags)
                .HasForeignKey(d => d.TagId)
                .HasConstraintName("restaurant_tags_tag_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");
            entity.ToTable("roles");

            entity.HasIndex(e => e.Name, "roles_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(20).HasColumnName("name");
        });

        modelBuilder.Entity<SignatureFood>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("signature_foods_pkey");
            entity.ToTable("signature_foods");

            entity.HasIndex(e => e.RestaurantId, "idx_signature_foods_restaurant");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RestaurantId).IsRequired().HasColumnName("restaurant_id");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.Description).IsRequired().HasColumnName("description");
            entity.Property(e => e.ReferencePrice).HasPrecision(12, 2).HasColumnName("reference_price");
            entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(255).HasColumnName("image_url");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired().HasColumnName("is_deleted");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.SignatureFoods)
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("signature_foods_restaurant_id_fkey");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subscriptions_pkey");

            entity.ToTable("subscriptions");

            entity.HasIndex(e => new { e.UserId, e.IsActive, e.StartAt, e.EndAt }, "idx_subscriptions_user_active");
            entity.HasIndex(e => new { e.UserId, e.StartAt }, "idx_subscriptions_user_start");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.EndAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("end_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.StartAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Plan).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("subscriptions_plan_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("subscriptions_user_id_fkey");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tags_pkey");
            entity.ToTable("tags");

            entity.HasIndex(e => e.CategoryId, "idx_tags_category");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50).HasColumnName("name");
            entity.Property(e => e.CategoryId).IsRequired().HasColumnName("category_id");
            entity.Property(e => e.Description).IsRequired().HasColumnName("description");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired().HasColumnName("is_deleted");

            entity.HasOne(d => d.Category).WithMany(p => p.Tags)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("tags_category_id_fkey");
        });

        modelBuilder.Entity<Transaction>(static entity =>
        {

            entity.HasKey(e => e.Id).HasName("transactions_pkey");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.UserId, "idx_transactions_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AmountCents).HasColumnName("amount_cents");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(8)
                .HasDefaultValueSql("'VND'::character varying")
                .HasColumnName("currency");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.FailureReason).HasColumnName("failure_reason");
            entity.Property(t => t.Provider).HasColumnType("payment_provider").HasColumnName("provider");
            entity.Property(t => t.Status).HasColumnType("payment_status").HasColumnName("status");
            entity.Property(e => e.IdempotencyKey)
                .HasMaxLength(255)
                .HasColumnName("idempotency_key");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.IsSettled)
                .HasDefaultValue(false)
                .HasColumnName("is_settled");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.PaymentMethodId).HasColumnName("payment_method_id");
            entity.Property(e => e.ProviderTransactionId)
                .HasMaxLength(255)
                .HasColumnName("provider_transaction_id");
            entity.Property(e => e.RefundedAmountCents)
                .HasDefaultValue(0L)
                .HasColumnName("refunded_amount_cents");
            entity.Property(e => e.RestaurantId).HasColumnName("restaurant_id");
            entity.Property(e => e.SettledAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("settled_at");
            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Uuid)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("uuid");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("transactions_invoice_id_fkey");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("transactions_payment_method_id_fkey");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("transactions_restaurant_id_fkey");

            entity.HasOne(d => d.Subscription).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("transactions_subscription_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("transactions_user_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "idx_users_email").IsUnique();
            entity.HasIndex(e => e.Username, "idx_users_username");
            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();
            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();
            entity.HasIndex(e => e.GoogleId, "ux_users_google_id")
                .IsUnique()
                .HasFilter("(google_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(255)
                .HasColumnName("avatar_url");
            entity.Property(e => e.BookmarkDowngradedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("bookmark_downgraded_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerifiedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("email_verified_at");
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.Gender)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.GoogleId)
                .HasMaxLength(64)
                .HasColumnName("google_id");
            entity.Property(e => e.Status)
                .HasColumnType("user_status")
                .HasColumnName("status")
                .IsRequired();
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsEmailVerified)
                .HasDefaultValue(false)
                .HasColumnName("is_email_verified");
            entity.Property(e => e.IsPhoneVerified)
                .HasDefaultValue(false)
                .HasColumnName("is_phone_verified");
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(30)
                .HasColumnName("phone_number");
            entity.Property(e => e.PhoneVerifiedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("phone_verified_at");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_role_id_fkey");
        });

        modelBuilder.Entity<UserActivePlan>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("user_active_plan");

            entity.Property(e => e.AdvancedFilters).HasColumnName("advanced_filters");
            entity.Property(e => e.AiChatLevel)
                .HasMaxLength(20)
                .HasColumnName("ai_chat_level");
            entity.Property(e => e.BookmarkLimit).HasColumnName("bookmark_limit");
            entity.Property(e => e.EndAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("end_at");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.MonthlyPostLimit).HasColumnName("monthly_post_limit");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Personalization).HasColumnName("personalization");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.RoleScope)
                .HasMaxLength(20)
                .HasColumnName("role_scope");
            entity.Property(e => e.Slug)
                .HasMaxLength(50)
                .HasColumnName("slug");
            entity.Property(e => e.StartAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<UserLocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_locations_pkey");
            entity.ToTable("user_locations");

            entity.HasIndex(e => e.UserId, "idx_user_locations_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Latitude).IsRequired().HasColumnName("latitude");
            entity.Property(e => e.Longitude).IsRequired().HasColumnName("longitude");
            entity.Property(e => e.Address).IsRequired().HasMaxLength(255).HasColumnName("address");
            entity.Property(e => e.LocationType).IsRequired().HasMaxLength(20).HasColumnName("location_type");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("created_at");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired().HasColumnName("is_deleted");

            entity.HasOne(d => d.User).WithMany(p => p.UserLocations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_locations_user_id_fkey");
        });

        modelBuilder.Entity<UserPostInteraction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_post_interactions_pkey");

            entity.ToTable("user_post_interactions");

            entity.HasIndex(e => e.PostId, "idx_user_post_interactions_post");
            entity.HasIndex(e => e.PostId, "idx_user_post_interactions_post_not_deleted").HasFilter("(is_deleted = false)");
            entity.HasIndex(e => e.RestaurantId, "idx_user_post_interactions_restaurant");
            entity.HasIndex(e => e.InteractionType, "idx_user_post_interactions_type");
            entity.HasIndex(e => e.UserId, "idx_user_post_interactions_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Detail)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("detail");
            entity.Property(e => e.InteractionType)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValueSql("'view'::character varying")
                .HasColumnName("interaction_type");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.RestaurantId).HasColumnName("restaurant_id");
            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Post).WithMany(p => p.UserPostInteractions)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("user_post_interactions_post_id_fkey");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.UserPostInteractions)
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_post_interactions_restaurant_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserPostInteractions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_post_interactions_user_id_fkey");
        });

        modelBuilder.Entity<UserRestaurantInteraction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_restaurant_interactions_pkey");
            entity.ToTable("user_restaurant_interactions");

            entity.HasIndex(e => e.RestaurantId, "idx_user_restaurant_interactions_restaurant");
            entity.HasIndex(e => e.UserId, "idx_user_restaurant_interactions_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).IsRequired().HasColumnName("user_id");
            entity.Property(e => e.RestaurantId).HasColumnName("restaurant_id");
            entity.Property(e => e.LastInteractionAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("last_interaction_at");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Bookmarked).HasDefaultValue(false).HasColumnName("bookmarked");
            entity.Property(e => e.VisitCount).HasDefaultValue(0).HasColumnName("visit_count");
            entity.Property(e => e.FavoriteFoodId).HasColumnName("favorite_food_id");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired().HasColumnName("is_deleted");

            entity.HasOne(d => d.FavoriteFood).WithMany(p => p.UserRestaurantInteractions)
                .HasForeignKey(d => d.FavoriteFoodId)
                .HasConstraintName("user_restaurant_interactions_favorite_food_id_fkey");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.UserRestaurantInteractions)
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_restaurant_interactions_restaurant_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserRestaurantInteractions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_restaurant_interactions_user_id_fkey");
        });

        modelBuilder.Entity<UserTag>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.TagId }).HasName("user_tags_pkey");
            entity.ToTable("user_tags");

            entity.HasIndex(e => e.TagId, "idx_user_tags_tag");
            entity.HasIndex(e => e.UserId, "idx_user_tags_user");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TagId).HasColumnName("tag_id");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).HasColumnName("is_deleted");

            entity.HasOne(d => d.Tag).WithMany(p => p.UserTags)
                .HasForeignKey(d => d.TagId)
                .HasConstraintName("user_tags_tag_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserTags)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_tags_user_id_fkey");
        });

        modelBuilder.Entity<WeatherLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("weather_logs_pkey");
            entity.ToTable("weather_logs");

            entity.HasIndex(e => e.UserId, "idx_weather_logs_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).IsRequired().HasColumnName("user_id");
            entity.Property(e => e.Latitude).IsRequired().HasColumnName("latitude");
            entity.Property(e => e.Longitude).IsRequired().HasColumnName("longitude");
            entity.Property(e => e.WeatherData).IsRequired().HasColumnType("jsonb").HasColumnName("weather_data");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("created_at");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired().HasColumnName("is_deleted");

            entity.HasOne(d => d.User).WithMany(p => p.WeatherLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("weather_logs_user_id_fkey");
        });

        modelBuilder.Entity<WebhookEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("webhook_events_pkey");

            entity.ToTable("webhook_events");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EventType)
                .HasMaxLength(200)
                .HasColumnName("event_type");
            entity.Property(e => e.Processed)
                .HasDefaultValue(false)
                .HasColumnName("processed");
            entity.Property(e => e.ProcessedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("processed_at");
            entity.Property(e => e.ProcessingResult)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("processing_result");
            entity.Property(e => e.ProviderEventId)
                .HasMaxLength(255)
                .HasColumnName("provider_event_id");
            entity.Property(e => e.RawPayload)
                .HasColumnType("jsonb")
                .HasColumnName("raw_payload");
            entity.Property(e => e.ReceivedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("received_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}