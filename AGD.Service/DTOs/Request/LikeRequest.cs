using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.DTOs.Request
{
    public class LikeRequest
    {
        public int UserId { get; set; }

        public int PostId { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
