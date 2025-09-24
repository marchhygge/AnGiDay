using AGD.Service.DTOs.Request;
using AGD.Service.DTOs.Response;
using AGD.Service.Services.Interfaces;
using AGD.Service.Shared.Result;
using Microsoft.AspNetCore.Mvc;

namespace AGD.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;
        public StorageController(IServicesProvider servicesProvider)
        {
            _servicesProvider = servicesProvider;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(52428800)]
        public async Task<ActionResult<ApiResult<StorageUploadResponse>>> Upload([FromForm] StorageUploadRequest request, CancellationToken ct = default)
        {
            if (request.File == null || request.File.Length == 0) return BadRequest("File rỗng");
            request.Key ??= $"uploads/{Guid.NewGuid()}_{request.File.FileName}";
            await using var stream = request.File.OpenReadStream();
            var etag = await _servicesProvider.ObjectStorageService.UploadAsync(request.Key, stream, request.File.ContentType, ct);
            var response = new StorageUploadResponse
            {
                Key = request.Key,
                Etag = etag
            };

            return ApiResult<StorageUploadResponse>.SuccessResponse(response);
        }

        [HttpPost("presign-upload")]
        public ActionResult<ApiResult<PresignUploadResponse>> PresignUpload([FromQuery] PresignUploadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FileName))
            {
                return ApiResult<PresignUploadResponse>.FailResponse("File name là bắt buộc");
            }
            var key = string.IsNullOrWhiteSpace(request.Prefix) ? request.FileName : $"{request.Prefix!.TrimEnd('/')}/{request.FileName}";
            var url = _servicesProvider.ObjectStorageService.GetPreSignedUploadUrl(key, null, request.ContentType);

            var response = new PresignUploadResponse
            {
                Key = key,
                Url = url,
                Method = "PUT",
                ContentType = request.ContentType
            };

            return ApiResult<PresignUploadResponse>.SuccessResponse(response);
        }

        [HttpGet("presign-download")]
        public ActionResult<ApiResult<PresignDownloadResponse>> PresignDownload([FromQuery] PresignDownloadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Key))
            {
                return ApiResult<PresignDownloadResponse>.FailResponse("Key là bắt buộc.");
            }
            var url = _servicesProvider.ObjectStorageService
                    .GetPreSignedDownloadUrl(request.Key, null, request.FileName ?? Path.GetFileName(request.Key), request.ContentType);

            var response = new PresignDownloadResponse
            {
                Key = request.Key,
                Url = url,
                Method = "GET",
            };
            return ApiResult<PresignDownloadResponse>.SuccessResponse(response);
        }

        [HttpGet("view")]
        public async Task View([FromQuery] string key, CancellationToken ct = default)
        {
            using var obj = await _servicesProvider.ObjectStorageService.OpenReadAsync(key, ct);

            Response.StatusCode = StatusCodes.Status200OK;
            Response.ContentType = obj.ContentType;

            if (obj.ContentLength.HasValue)
            {
                Response.ContentLength = obj.ContentLength;
            }

            Response.Headers.ContentDisposition = $"inline; filename=\"{Path.GetFileName(key)}\"";

            await obj.Stream.CopyToAsync(Response.Body, ct);
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] string key, CancellationToken ct)
        {
            var (contentType, _) = await _servicesProvider.ObjectStorageService.GetMetadataAsync(key, ct);
            var stream = await _servicesProvider.ObjectStorageService.DownloadAsync(key, ct);
            var fileName = Path.GetFileName(key);
            return File(stream, "application/octet-stream", fileName);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] string key, CancellationToken ct)
        {
            await _servicesProvider.ObjectStorageService.DeleteAsync(key, ct);
            return NoContent();
        }

        [HttpGet("list")]
        public async Task<ActionResult<ApiResult<StorageListResponse>>> List([FromQuery] StorageListRequest request, CancellationToken ct = default)
        {
            var keys = await _servicesProvider.ObjectStorageService.ListAsync(request.Prefix, request.MaxKey, ct);
            var response = new StorageListResponse
            {
                Keys = keys,
            };
            return ApiResult<StorageListResponse>.SuccessResponse(response);
        }
    }
}
