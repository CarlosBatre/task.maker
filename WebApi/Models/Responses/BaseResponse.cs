using System.Net;

namespace WebApi.Models.Responses;

public class BaseResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public T Data { get; set; }
    public IEnumerable<string> Errors { get; set; }

    public BaseResponse()
    {
        Success = true;
        Message = string.Empty;
        StatusCode = HttpStatusCode.OK;
        Errors = new List<string>();
    }

    public static BaseResponse<T> Ok(T data, string message = "Operation completed successfully")
    {
        return new BaseResponse<T>
        {
            Success = true,
            Message = message,
            StatusCode = HttpStatusCode.OK,
            Data = data
        };
    }

    public static BaseResponse<T> Created(T data, string message = "Resource created successfully")
    {
        return new BaseResponse<T>
        {
            Success = true,
            Message = message,
            StatusCode = HttpStatusCode.Created,
            Data = data
        };
    }

    public static BaseResponse<T> Error(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        return new BaseResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = statusCode
        };
    }

    public static BaseResponse<T> BadRequest(string message = "Invalid request", IEnumerable<string> errors = null)
    {
        return new BaseResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = HttpStatusCode.BadRequest,
            Errors = errors ?? new List<string>()
        };
    }

    public static BaseResponse<T> NotFound(string message = "Resource not found")
    {
        return new BaseResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = HttpStatusCode.NotFound
        };
    }

    public static BaseResponse<T> Unauthorized(string message = "Unauthorized")
    {
        return new BaseResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = HttpStatusCode.Unauthorized
        };
    }
}