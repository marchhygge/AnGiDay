namespace AGD.Service.DTOs.Response
{
    public class PresignDownloadResponse
    {
        public string? Key { get; set; }
        public string? Url { get; set; }
        public string? Method { get; set; } = "GET";
    }
}
