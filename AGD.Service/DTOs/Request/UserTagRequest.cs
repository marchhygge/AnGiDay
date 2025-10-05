using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.DTOs.Request
{
    public class UserTagRequest
    {
        public int UserId { get; set; }

        public int TagId { get; set; }

        public bool? IsDeleted { get; set; } = false;
    }
}
