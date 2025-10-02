using AGD.Repositories.Enums;
using System.Text.Json.Serialization;

namespace AGD.Repositories.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool IsEmailVerified { get; set; } = false;

    public DateTime? EmailVerifiedAt { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public bool IsPhoneVerified { get; set; } = false;

    public DateTime? PhoneVerifiedAt { get; set; }

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public string FullName { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string Gender { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    public string? GoogleId { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTime? BookmarkDowngradedAt { get; set; }
    [JsonIgnore]
    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    [JsonIgnore]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    [JsonIgnore]
    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    [JsonIgnore]
    public virtual HealthProfile? HealthProfile { get; set; }
    [JsonIgnore]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    [JsonIgnore]
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    [JsonIgnore]
    public virtual ICollection<NotificationUser> NotificationUsers { get; set; } = new List<NotificationUser>();
    [JsonIgnore]
    public virtual ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
    [JsonIgnore]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    [JsonIgnore]
    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
    [JsonIgnore]
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    [JsonIgnore]
    public virtual ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
    [JsonIgnore]
    public virtual Role Role { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    [JsonIgnore]
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    [JsonIgnore]
    public virtual ICollection<UserLocation> UserLocations { get; set; } = new List<UserLocation>();
    [JsonIgnore]
    public virtual ICollection<UserPostInteraction> UserPostInteractions { get; set; } = new List<UserPostInteraction>();
    [JsonIgnore]    
    public virtual ICollection<UserRestaurantInteraction> UserRestaurantInteractions { get; set; } = new List<UserRestaurantInteraction>();
    [JsonIgnore]
    public virtual ICollection<UserTag> UserTags { get; set; } = new List<UserTag>();
    [JsonIgnore]
    public virtual ICollection<WeatherLog> WeatherLogs { get; set; } = new List<WeatherLog>();
}
