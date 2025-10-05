using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.DTOs.Response
{
    public class PostResponse
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? RestaurantId { get; set; }
        public string Type { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public int? Rating { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
