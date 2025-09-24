namespace AGD.Service.DTOs.Request
{
    public class StorageListRequest
    {
        public string? Prefix { get; set; } = null;
        public int? MaxKey { get; set; } = 100;
    }
}
