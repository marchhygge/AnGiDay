using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.DTOs.Response
{
    public class DetailPostResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public List<CommentDetailResponse> Comments { get; set; } = new List<CommentDetailResponse>();

    }

    public class CommentDetailResponse
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int? ParentId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        public List<CommentDetailResponse> Replies { get; set; } = new List<CommentDetailResponse>();
    }
}
