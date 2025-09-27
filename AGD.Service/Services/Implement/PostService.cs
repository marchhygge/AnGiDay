using AGD.Repositories.Models;
using AGD.Repositories.Repositories;
using AGD.Service.DTOs.Response;
using AGD.Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.Services.Implement
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        public PostService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Post?> GetAsync(int id, CancellationToken ct = default)
        {
            return await _unitOfWork.PostRepository.GetByIdAsync(ct, id);
        }

        public IQueryable<Post> GetRestaurantPost(int resId)
        {
            return _unitOfWork.PostRepository.GetRestaurantPost(resId);
        }

        public async Task<IEnumerable<FeedbackResponse>> GetRestaurantFeedback(int resId, CancellationToken ct = default)
        {
            var feedback = await _unitOfWork.PostRepository.GetRestaurantFeedback(resId, ct);

            return feedback.Select(p => new FeedbackResponse
            {
                Id = p.Id,
                Content = p.Content,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt,
                UserName = p.User.Username,
                SignatureFoodName = p.SignatureFood != null ? p.SignatureFood.Name : null
            });
        }

        public async Task<DetailPostResponse> GetPostDetail(int id, CancellationToken ct = default)
        {
            var post = await _unitOfWork.PostRepository.GetPostDetailAsync(id, ct);           

            var postRes = new DetailPostResponse
            {
                Id = post.Id,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                CreatedAt = post.CreatedAt,
                UserName = post.User.Username,
                IsDeleted = post.IsDeleted,
                Comments = GetCommentTree(post.Comments.Where(c => c.ParentId == null).ToList())
            };
            return postRes;
        }

        private List<CommentDetailResponse> GetCommentTree(List<Comment> comments)
        {
            return comments.Select(c => new CommentDetailResponse
            {
                Id = c.Id,
                Content = c.Content,
                PostId = c.PostId,
                UserId = c.UserId,
                UserName = c.User?.Username ?? "Unknown",
                ParentId = c.ParentId,
                CreatedAt = c.CreatedAt,
                Replies = GetCommentTree(c.InverseParent?.ToList() ?? new List<Comment>())
            }).ToList();
        }


    }
}
