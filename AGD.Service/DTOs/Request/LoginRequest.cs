using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.DTOs.Request
{
    public class LoginUserNameRequest
    {
        public string username {  get; set; } = null!;
        public string password { get; set; } = null!;
    }
}
