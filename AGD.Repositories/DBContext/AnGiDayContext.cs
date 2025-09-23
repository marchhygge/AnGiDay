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
            .HasPostgresEnum("public", "notification_type", new[] { "like", "comment", "report", "reply", "system" })
            .HasPostgresEnum<UserStatus>("public", "user_status");

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
            entity.Property(e => e.UserId).IsRequired().HasColumnName("user_id");
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("started_at");
            entity.Property(e => e.EndedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ended_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired()
                .HasColumnName("is_deleted");

            entity.HasOne(d => d.User).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("conversations_user_id_fkey");
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
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired().HasColumnName("is_deleted");

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

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ConversationId).IsRequired().HasColumnName("conversation_id");
            entity.Property(e => e.Sender).IsRequired().HasMaxLength(10).HasColumnName("sender");
            entity.Property(e => e.Content).IsRequired().HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired().HasColumnName("is_deleted");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .OnDelete(DeleteBehavior.Cascade)
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
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RestaurantId).HasColumnName("restaurant_id");
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20).HasColumnName("type");
            entity.Property(e => e.Content).IsRequired().HasColumnName("content");
            entity.Property(e => e.ImageUrl).HasMaxLength(255).HasColumnName("image_url");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.SignatureFoodId).HasColumnName("signature_food_id");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("updated_at");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired().HasColumnName("is_deleted");

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
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.Address).IsRequired().HasMaxLength(255).HasColumnName("address");
            entity.Property(e => e.OwnerId).IsRequired().HasColumnName("owner_id");
            entity.Property(e => e.Description).IsRequired().HasColumnName("description");
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(30).HasColumnName("phone_number");
            entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(255).HasColumnName("image_url");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValueSql("'active'::character varying").HasColumnName("status");
            entity.Property(e => e.Latitude).IsRequired().HasColumnName("latitude");
            entity.Property(e => e.Longitude).IsRequired().HasColumnName("longitude");
            entity.Property(e => e.AvgRating).HasPrecision(3, 2).HasDefaultValueSql("0").HasColumnName("avg_rating");
            entity.Property(e => e.RatingCount).HasDefaultValue(0).HasColumnName("rating_count");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("updated_at");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired().HasColumnName("is_deleted");

            entity.HasOne(d => d.Owner).WithMany(p => p.Restaurants)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.Restrict)
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

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");
            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "idx_users_email").IsUnique();
            entity.HasIndex(e => e.Username, "idx_users_username");
            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();
            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();
            entity.HasIndex(e => e.GoogleId, "ux_users_google_id").IsUnique().HasFilter("(google_id IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50).HasColumnName("username");
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100).HasColumnName("email");
            entity.Property(e => e.IsEmailVerified).HasDefaultValue(false).IsRequired().HasColumnName("is_email_verified");
            entity.Property(e => e.EmailVerifiedAt).HasColumnType("timestamp without time zone").HasColumnName("email_verified_at");
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(30).HasColumnName("phone_number");
            entity.Property(e => e.IsPhoneVerified).HasDefaultValue(false).IsRequired().HasColumnName("is_phone_verified");
            entity.Property(e => e.PhoneVerifiedAt).HasColumnType("timestamp without time zone").HasColumnName("phone_verified_at");
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255).HasColumnName("password_hash");
            entity.Property(e => e.RoleId).IsRequired().HasColumnName("role_id");
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100).HasColumnName("full_name");
            entity.Property(e => e.AvatarUrl).HasMaxLength(255).HasColumnName("avatar_url");
            entity.Property(e => e.Gender).IsRequired().HasMaxLength(10).HasColumnName("gender");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("updated_at");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired().HasColumnName("is_deleted");
            entity.Property(e => e.GoogleId).HasMaxLength(64).HasColumnName("google_id");
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasColumnType("user_status")
                  .HasColumnName("status");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("users_role_id_fkey");
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}