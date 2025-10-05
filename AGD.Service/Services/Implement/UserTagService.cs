using AGD.Repositories.Models;
using AGD.Repositories.Repositories;
using AGD.Service.DTOs.Request;
using AGD.Service.DTOs.Response;
using AGD.Service.Services.Interfaces;
using Amazon.Runtime.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.Services.Implement
{
    public class UserTagService : IUserTagService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserTagService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UserTagResponse>> AddUpdateUserTag(IEnumerable<UserTagRequest> requests, CancellationToken ct)
        {
            var responses = new List<UserTagResponse>();

            foreach (var request in requests)
            {
                var existing = await _unitOfWork.UserTagRepository
                    .GetUserTagAsync(request.UserId, request.TagId, ct);

                if (existing == null)
                {
                    existing = new UserTag
                    {
                        UserId = request.UserId,
                        TagId = request.TagId,
                        IsDeleted = false,
                    };

                    await _unitOfWork.UserTagRepository.AddUserTagAsync(existing, ct);
                }
                else
                {
                    existing.IsDeleted = request.IsDeleted ?? false;
                    await _unitOfWork.UserTagRepository.UpdateUserTagAsync(existing, ct);
                }

                var tag = await _unitOfWork.TagRepository.GetByIdAsync(ct, request.TagId);

                responses.Add(new UserTagResponse
                {
                    UserId = existing.UserId,
                    TagId = existing.TagId,
                    TagName = tag?.Name ?? string.Empty,
                    IsDeleted = existing.IsDeleted
                });
            }

            return responses;
        }

        public async Task<IEnumerable<UserTagResponse>> GetTagsOfUserAsync(int userId, CancellationToken ct)
        {
            var result = await _unitOfWork.UserTagRepository.GetTagsOfUserAsync(userId, ct);

            return result.Select(r => new UserTagResponse
            {
                UserId = r.UserId,
                TagId = r.TagId,
                TagName = r.Tag.Name,
                CategoryName = r.Tag.Category.Name,
                IsDeleted = r.IsDeleted,
            });
        }
    }
}
