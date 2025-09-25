namespace AGD.Service.Services.Interfaces
{
    public interface IObjectStorageService
    {
        Task<string> UploadAsync(string key, Stream content, string? contentType, CancellationToken ct = default);
        Task<(string ContentType, long ContentLength)> GetMetadataAsync(string key, CancellationToken ct = default);
        Task<Stream> DownloadAsync(string key, CancellationToken ct = default);
        Task<ObjectDownloadResult> OpenReadAsync(string key, CancellationToken ct = default);
        Task DeleteAsync(string key, CancellationToken ct = default);
        Task<IReadOnlyList<string>> ListAsync(string? prefix = null, int? maxKeys = null, CancellationToken ct = default);
        string GetPreSignedUploadUrl(string key, TimeSpan? expiresIn = null, string? contentType = null);
        string GetPreSignedDownloadUrl(string key, TimeSpan? expiresIn = null, string? downloadFileName = null, string? contentType = null);
        string GetBaseUrl();
    }
}
