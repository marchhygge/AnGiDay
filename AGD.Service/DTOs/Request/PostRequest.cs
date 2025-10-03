using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.DTOs.Request
{
    public class PostRequest
    {
        public int UserId { get; set; }
        public int? RestaurantId { get; set; }
        public string Type { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? Rating { get; set; }
    }
}
