using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Core.Store;

namespace Core.Base
{

    public class BaseException : Exception
    {
        public class CoreException : Exception
        {
            public CoreException(string code, string message, int statusCode = (int)StatusCodeHelper.ServerError)
                : base(message)
            {
                Code = code;
                StatusCode = statusCode;
            }


            public string Code { get; }

            public int StatusCode { get; set; }

            public Dictionary<string, object>? AdditionalData { get; set; }
        }
        public class BadRequestException : ErrorException
        {
            public BadRequestException(string errorCode, string message)
                : base(400, errorCode, message)
            {
            }
            public BadRequestException(
                ICollection<KeyValuePair<string, ICollection<string>>> errors)
                : base(400, new ErrorDetail
                {
                    ErrorCode = "bad_request",
                    ErrorMessage = errors
                })
            {
            }
        }

        public class ErrorException : Exception
        {
            public int StatusCode { get; }

            public ErrorDetail ErrorDetail { get; }

            public ErrorException(int statusCode, string errorCode, string message)
            {
                StatusCode = statusCode;
                ErrorDetail = new ErrorDetail
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                };
            }

            public ErrorException(int statusCode, ErrorDetail errorDetail)
            {
                StatusCode = statusCode;
                ErrorDetail = errorDetail;
            }
        }

        public class ErrorDetail
        {
            [JsonPropertyName("errorCode")] public string? ErrorCode { get; set; }

            [JsonPropertyName("errorMessage")] public object? ErrorMessage { get; set; }
        }
    }
}
