namespace AGD.Service.DTOs.Request
{
    public class PresignDownloadRequest
    {
        public string? Key { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
    }
}
