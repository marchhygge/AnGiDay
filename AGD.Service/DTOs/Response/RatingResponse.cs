using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.DTOs.Response
{
    public class RatingResponse
    {
        public int UserId { get; set; }

        public int PostId { get; set; }

        public int? Rating { get; set; }

        public DateTime? CreatedAt { get; set; }

        public bool IsDeleted { get; set; }
    }
}
