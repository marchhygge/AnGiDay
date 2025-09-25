using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.DTOs.Response
{
    public class FeedbackResponse
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }

        public string UserName { get; set; } = null!;
        public string? SignatureFoodName { get; set; }
    }
}
