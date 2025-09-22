using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.Shared.Result
{
    public class ApiResult<T>
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ApiResult<T> SuccessResponse(T data, string message = "Success", int statusCode = 200)
        {
            return new ApiResult<T>
            {
                IsSuccess = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static ApiResult<T> FailResponse(string message = "Failed", int statusCode = 400)
        {
            return new ApiResult<T>
            {
                IsSuccess = false,
                StatusCode = statusCode,
                Message = message,
                Data = default
            };
        }
    }
}
