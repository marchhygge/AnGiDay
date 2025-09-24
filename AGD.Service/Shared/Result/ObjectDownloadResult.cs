using Amazon.S3.Model;

public sealed class ObjectDownloadResult : IDisposable
{
    internal GetObjectResponse? _response;

    public Stream Stream { get; }
    public string ContentType { get; }
    public long? ContentLength { get; }
    public string? ETag { get; }

    public ObjectDownloadResult(Stream stream, string contentType, long? contentLength, string? eTag)
    {
        Stream = stream;
        ContentType = contentType;
        ContentLength = contentLength;
        ETag = eTag;
    }

    public void Dispose()
    {
        _response?.Dispose();
    }
}