using Core.Store;
using Core.Utils;

namespace Core.Base
{
    public class BaseResponse<T>
    {
        public T? Data { get; set; }
        public string? Message { get; set; }
        public StatusCodeHelper StatusCode { get; set; }
        public string? Code { get; set; }
        public bool IsSuccess => int.Parse(Code) >= 200 && int.Parse(Code) < 300;
        public BaseResponse(StatusCodeHelper statusCode, string code, T? data, string? message)
        {
            Data = data;
            Message = message;
            StatusCode = statusCode;
            Code = code;
        }

        public BaseResponse(StatusCodeHelper statusCode, string code, T? data)
        {
            Data = data;
            StatusCode = statusCode;
            Code = code;
        }

        public BaseResponse(StatusCodeHelper statusCode, string code, string? message)
        {
            Message = message;
            StatusCode = statusCode;
            Code = code;
        }

        public static BaseResponse<T> OkResponse(T? data)
        {
            return new BaseResponse<T>(StatusCodeHelper.OK, StatusCodeHelper.OK.Name(), data);
        }
        public static BaseResponse<T> OkResponse(string? mess)
        {
            return new BaseResponse<T>(StatusCodeHelper.OK, StatusCodeHelper.OK.Name(), mess);
        }
    }
}
