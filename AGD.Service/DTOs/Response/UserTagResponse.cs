using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.DTOs.Response
{
    public class UserTagResponse
    {
        public int UserId { get; set; }

        public int TagId { get; set; }

        public string TagName { get; set; } = null!;

        public string CategoryName { get; set; } = null!;

        public bool? IsDeleted { get; set; } = false;
    }
}
