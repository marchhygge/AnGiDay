using Microsoft.AspNetCore.Http;

namespace AGD.Service.DTOs.Request
{
    public class StorageUploadRequest
    {
        public IFormFile File { get; set; } = null!;
        public string? Key { get; set; }
    }
}
