using AGD.Repositories.ConfigurationModels;
using AGD.Service.Services.Interfaces;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace AGD.Service.Services.Implement
{
    public class R2StorageService : IObjectStorageService
    {
        private readonly R2Options _options;
        private readonly IAmazonS3 _s3;

        public R2StorageService(IOptions<R2Options> options)
        {
            _options = options.Value;

            var config = new AmazonS3Config
            {
                ServiceURL = $"https://{_options.AccountId}.r2.cloudflarestorage.com",
                ForcePathStyle = true,
                AuthenticationRegion = "auto",
                UseHttp = false,
            };
            var credentials = new BasicAWSCredentials(_options.AccessKeyId, _options.SecretAccessKey);
            _s3 = new AmazonS3Client(credentials, config);
        }

        public async Task DeleteAsync(string key, CancellationToken ct = default)
        {
            await _s3.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key
            }, ct);
        }

        public async Task<Stream> DownloadAsync(string key, CancellationToken ct = default)
        {
            var response = await _s3.GetObjectAsync(new GetObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key
            }, ct);

            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream, ct);
            memoryStream.Position = 0;
            return memoryStream;
        }

        public async Task<(string ContentType, long ContentLength)> GetMetadataAsync(string key, CancellationToken ct = default)
        {
            var meta = await _s3.GetObjectMetadataAsync(_options.BucketName, key, ct);
            return (meta.Headers.ContentType ?? "application/octet-stream", meta.ContentLength);
        }

        public async Task<ObjectDownloadResult> OpenReadAsync(string key, CancellationToken ct = default)
        {
            var resp = await _s3.GetObjectAsync(new GetObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key
            }, ct);

            var result = new ObjectDownloadResult(
                stream: resp.ResponseStream,
                contentType: resp.Headers.ContentType ?? "application/octet-stream",
                contentLength: resp.Headers.ContentLength >= 0 ? resp.Headers.ContentLength : null,
                eTag: resp.ETag?.Trim('"')
            )
            {
                _response = resp
            };
            return result;
        }

        public string GetPreSignedUploadUrl(string key, TimeSpan? expiresIn = null, string? contentType = null)
        {
            var expiry = expiresIn ?? TimeSpan.FromMinutes(_options.PresignExpiryMinutes);
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                Expires = DateTime.UtcNow.Add(expiry),
                Verb = HttpVerb.PUT
            };
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                request.Headers["Content-Type"] = contentType;
            }
            return _s3.GetPreSignedURL(request);
        }

        public string GetPreSignedDownloadUrl(string key, TimeSpan? expiresIn = null, string? downloadFileName = null, string? contentType = null)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(expiresIn ?? TimeSpan.FromMinutes(_options.PresignExpiryMinutes)),
                ResponseHeaderOverrides = new ResponseHeaderOverrides()
            };

            if (!string.IsNullOrWhiteSpace(downloadFileName))
            {
                request.ResponseHeaderOverrides.ContentDisposition = $"attachment; filename=\"{downloadFileName}\"";
            }

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                request.ResponseHeaderOverrides.ContentType = contentType;
            }

            return _s3.GetPreSignedURL(request);
        }

        public async Task<IReadOnlyList<string>> ListAsync(string? prefix = null, int? maxKeys = null, CancellationToken ct = default)
        {
            var response = await _s3.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = _options.BucketName,
                Prefix = prefix ?? string.Empty,
                MaxKeys = maxKeys ?? 1000
            }, ct);

            return response.S3Objects.Select(o => o.Key).ToList();
        }

        public async Task<string> UploadAsync(string key, Stream content, string? contentType, CancellationToken ct = default)
        {
            var url = _s3.GetPreSignedURL(new Amazon.S3.Model.GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                Verb = Amazon.S3.HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(_options.PresignExpiryMinutes)
            });

            Stream payload = content;
            if (!content.CanSeek)
            {
                var memoryStream = new MemoryStream();
                await content.CopyToAsync(memoryStream, ct);
                memoryStream.Position = 0;
                payload = memoryStream;
            }
            else if (content.Position != 0)
            {
                content.Position = 0;
            }

            using var http = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Content = new StreamContent(payload);
            if (payload.CanSeek) request.Content.Headers.ContentLength = payload.Length;
            if (!string.IsNullOrWhiteSpace(contentType))
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            using var res = await http.SendAsync(request, ct);
            res.EnsureSuccessStatusCode();

            var etag = res.Headers.TryGetValues("ETag", out var vals) ? vals.FirstOrDefault()?.Trim('"') : null;
            return etag ?? string.Empty;
        }

        public string GetBaseUrl()
        {
            return _options.BaseUrl;
        }
    }
}
