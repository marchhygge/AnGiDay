using AGD.Repositories.Models;
using AGD.Repositories.Repositories;
using AGD.Service.DTOs.Request;
using AGD.Service.DTOs.Response;
using AGD.Service.Services.Interfaces;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

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
                Id = post!.Id,
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

        public async Task<LikeResponse> AddLikeAsync(LikeRequest request, CancellationToken ct = default)
        {
            var post = await _unitOfWork.PostRepository.GetByIdAsync(ct, request.PostId);

            if (post == null)
            {
                throw new Exception("Post not found");
            }

            if (post.Type.Equals("review", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("You cannot like review posts");
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(ct, request.UserId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var interaction = await _unitOfWork.PostRepository.GetByUserAndPostId(request.UserId, request.PostId, ct);

            if (interaction == null)
            {

                interaction = new Like
                {
                    UserId = request.UserId,
                    PostId = request.PostId,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false,
                };

                await _unitOfWork.PostRepository.AddLikePost(interaction, ct);
            }
            else
            {
                if(!interaction.IsDeleted)
                {
                    interaction.IsDeleted = true;
                }
                else
                {
                    interaction.IsDeleted = !interaction.IsDeleted;
                }
                    await _unitOfWork.PostRepository.UpdateLikePost(interaction, ct);
            }

            var totalLikes = await _unitOfWork.PostRepository.CountLikesByPostIdAsync(request.PostId, ct);

            return new LikeResponse
            {
                UserId = interaction.UserId,
                PostId = interaction.PostId,
                CreatedAt = DateTime.Now,
                IsDeleted = interaction.IsDeleted,
                TotalLikes = totalLikes
            };
        }

        public async Task<PostResponse> CreatePostAsync(PostRequest request, CancellationToken ct = default)
        {
            if (request.Type == "review")
            {
                if (!request.RestaurantId.HasValue)
                {
                    throw new Exception("RestaurantId is required for review posts");
                }

                var restaurant = await _unitOfWork.RestaurantRepository
                    .GetByIdAsync(ct, request.RestaurantId.Value);

                if (restaurant == null)
                    throw new Exception("Invalid RestaurantId");

                if (!request.Rating.HasValue)
                {
                    throw new Exception("Rating is required for review posts");
                }

                if (request.Rating < 1 || request.Rating > 5)
                    throw new Exception("Rating must be between 1 and 5");
            }
            else if(request.Type == "owner_post")
            {
                if (!request.RestaurantId.HasValue)
                {
                    throw new Exception("RestaurantId is required for owner posts");
                }

                var restaurant = await _unitOfWork.RestaurantRepository
                    .GetByIdAsync(ct, request.RestaurantId.Value);

                if (restaurant == null)
                    throw new Exception("Invalid RestaurantId");
            }
            else if (request.Type == "community_post")
            {
                request.RestaurantId = null;
            }

            var post = new Post
            {
                UserId = request.UserId,
                RestaurantId = request.RestaurantId,
                Type = request.Type,
                Content = request.Content,
                ImageUrl = request.ImageUrl,
                Rating = request.Type == "review" ? request.Rating : null,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IsDeleted = false
            };

            var created = await _unitOfWork.PostRepository.CreatePostAsync(post, ct);

            return new PostResponse
            {
                Id = created.Id,
                UserId = created.UserId,
                RestaurantId = created.RestaurantId,
                Type = created.Type,
                Content = created.Content,
                ImageUrl = created.ImageUrl,
                Rating = created.Rating,
                CreatedAt = created.CreatedAt ?? DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UpdatedAt = created.UpdatedAt ?? DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),     
            };
        }

        public async Task<IEnumerable<PostResponse>> GetPostsByTypeAsync(string type, CancellationToken ct)
        {
            var posts = await _unitOfWork.PostRepository.GetPostsByTypeAsync(type, ct);
            return posts.Select(p => new PostResponse
            {
                Id = p.Id,
                UserId = p.UserId,
                RestaurantId = p.RestaurantId,
                Type = p.Type,
                Content = p.Content,
                ImageUrl = p.ImageUrl,
                Rating = p.Rating,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });
        }

        public async Task<PostResponse?> UpdatePostAsync(int postId, PostRequest request, CancellationToken ct)
        {
            var existing = await _unitOfWork.PostRepository.GetPostDetailAsync(postId, ct);
            if (existing == null)
            {
                throw new Exception("Post not found");
            }

            existing.Content = request.Content ?? existing.Content;
            existing.ImageUrl = request.ImageUrl ?? existing.ImageUrl;
            existing.UpdatedAt = DateTime.Now;

            if(existing.Type == "review")
            {
                if(request.Rating == null)
                {
                    throw new Exception("Review post must have rating.");
                }
                existing.Rating = request.Rating;
            }

            await _unitOfWork.PostRepository.UpdatePostAsync(existing, ct);

            return new PostResponse
            {
                Id = existing.Id,
                UserId = existing.UserId,
                RestaurantId = existing.RestaurantId,
                Type = existing.Type,
                Content = existing.Content,
                ImageUrl = existing.ImageUrl,
                Rating = existing.Rating,
                CreatedAt = existing.CreatedAt,
                UpdatedAt = existing.UpdatedAt
            };
        }

        public async Task<bool> DeletePostAsync(int postId, CancellationToken ct)
        {
            var existing = await _unitOfWork.PostRepository.GetPostDetailAsync(postId, ct);
            if (existing == null) return false;

            await _unitOfWork.PostRepository.SoftDeletePostAsync(existing, ct);

            return true;
        }
    }
}
