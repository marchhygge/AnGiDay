using Microsoft.AspNetCore.Http;

namespace AGD.Service.DTOs.Request
{
    public class UpdateAvatarRequest
    {
        public IFormFile Avatar { get; set; } = null!;
    }
}
