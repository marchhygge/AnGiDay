using AGD.Repositories.Models;
using AGD.Service.DTOs.Request;
using AGD.Service.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.Services.Interfaces
{
    public interface IPostService
    {
        IQueryable<Post> GetRestaurantPost(int resId);
        Task<IEnumerable<FeedbackResponse>> GetRestaurantFeedback(int resId, CancellationToken ct = default);
        Task<Post?> GetAsync(int id, CancellationToken ct = default);
        Task<DetailPostResponse> GetPostDetail(int id, CancellationToken ct = default);
        Task<LikeResponse> AddLikeAsync(LikeRequest request, CancellationToken ct = default);
        Task<PostResponse> CreatePostAsync(PostRequest request, CancellationToken ct = default);
        Task<IEnumerable<PostResponse>> GetPostsByTypeAsync(string type, CancellationToken ct);
        Task<PostResponse?> UpdatePostAsync(int postId, PostRequest request, CancellationToken ct);
        Task<bool> DeletePostAsync(int postId, CancellationToken ct);
    }
}
