namespace AGD.Service.DTOs.Response
{
    public class PresignUploadResponse
    {
        public string? Key { get; set; }
        public string? Url { get; set; }
        public string? Method { get; set; } = "PUT";
        public string? ContentType { get; set; }
    }
}
