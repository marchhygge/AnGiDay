namespace AGD.Repositories.Models
{
    public partial class UserActivePlan
    {
        public int? UserId { get; set; }

        public int? PlanId { get; set; }

        public string Slug { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string RoleScope { get; set; } = string.Empty;

        public int? BookmarkLimit { get; set; }

        public int? MonthlyPostLimit { get; set; }

        public string AiChatLevel { get; set; } = string.Empty;

        public bool? Personalization { get; set; }

        public bool? AdvancedFilters { get; set; }

        public DateTime? StartAt { get; set; }

        public DateTime? EndAt { get; set; }

        public bool? IsActive { get; set; }
    }
}
