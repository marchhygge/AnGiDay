namespace AGD.Repositories.ConfigurationModels
{
    public class GoogleIdTokenOptions
    {
        public List<string> ClientIds { get; set; } = new List<string>();
        public bool RequireEmailVerified { get; set; } = true;
        public List<string> AllowedHostedDomains { get; set; } = new List<string>();
        public bool AutoCreateUser { get; set; } = true;
        public int DefaultRoleId { get; set; } = 2; // user role
        public string DefaultGender { get; set; } = "unknown";
    }
}
