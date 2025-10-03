using AGD.Repositories.Models;
using AGD.Service.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.Services.Interfaces
{
    public interface ITagService
    {
        Task<IEnumerable<TagResponse>> GetTags(CancellationToken ct = default);
    }
}
