namespace AGD.Service.DTOs.Request
{
    public class PresignUploadRequest
    {
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public string? Prefix { get; set; } = "uploads";
    }
}
