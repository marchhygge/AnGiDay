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
    public interface IUserTagService
    {
        Task<IEnumerable<UserTagResponse>> GetTagsOfUserAsync(int userId, CancellationToken ct);
        Task<IEnumerable<UserTagResponse>> AddUpdateUserTag(IEnumerable<UserTagRequest> request, CancellationToken ct);
    }
}
