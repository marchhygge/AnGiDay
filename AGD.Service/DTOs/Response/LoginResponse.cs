using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.DTOs.Response
{
    public class LoginUserNameResponse
    {
        public string AccessToken { get; set; } = null!;
        public string TokenType { get; set; } = "Bearer";
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public int RoleId { get; set; }
        public string Email { get; set; } = null!;
    }
}
