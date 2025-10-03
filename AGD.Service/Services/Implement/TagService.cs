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
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;
        public TagService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TagResponse>> GetTags(CancellationToken ct = default)
        {
            var tags = await _unitOfWork.TagRepository.GetTagAsync(ct);

            return tags.Select(t => new TagResponse
            {
                Id = t.Id,
                Name = t.Name,
                CategoryId = t.CategoryId,
                CategoryName = t.Category.Name,
                Description = t.Description,
                IsDeleted = t.IsDeleted,
            });
        }
    }
}
