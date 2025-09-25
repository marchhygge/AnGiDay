namespace AGD.Service.DTOs.Response
{
    public class UserProfileResponse
    {
        // Mandatory
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; } = string.Empty;

        // Optional
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public int RoleId { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public int TotalPosts { get; set; }
        public int TotalPostBookmarks { get; set; }
        public int TotalRestaurantBookmarks { get; set; }
        public int TotalRestaurantsOwned { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;

        // Convenience flag
        public bool IsProfileComplete { get; set; }
    }
}
