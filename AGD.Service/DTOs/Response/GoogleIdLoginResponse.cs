namespace AGD.Service.DTOs.Response
{
    public class GoogleIdLoginResponse
    {
        public string AccessToken { get; set; } = null!;
        public string TokenType { get; set; } = "Bearer";
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsNewUser { get; set; }
    }
}
