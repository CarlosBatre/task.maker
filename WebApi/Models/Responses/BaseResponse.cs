using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApi.Models.Responses;

public class BaseResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public T? Data { get; set; }
    public IEnumerable<string> Errors { get; set; }

    public BaseResponse()
    {
        Success = true;
        Message = string.Empty;
        StatusCode = HttpStatusCode.OK;
        Errors = new List<string>();
    }

    public static BaseResponse<T> Ok(T? data, string message = "Operación realizada exitosamente")
    {
        return new BaseResponse<T>
        {
            Success = true,
            Message = message,
            StatusCode = HttpStatusCode.OK,
            Data = data
        };
    }

    public static BaseResponse<T> Created(T? data, string message = "Recurso creado exitosamente")
    {
        return new BaseResponse<T>
        {
            Success = true,
            Message = message,
            StatusCode = HttpStatusCode.Created,
            Data = data
        };
    }

    public static BaseResponse<T> Error(string message = "Error interno del servidor", HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        return new BaseResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = statusCode
        };
    }

    public static BaseResponse<T> BadRequest(string message = "La solicitud no es válida", IEnumerable<string>? errors = null)
    {
        return new BaseResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = HttpStatusCode.BadRequest,
            Errors = errors ?? new List<string>()
        };
    }

    public static BaseResponse<T> ValidationError(ModelStateDictionary modelState, string message = "La solicitud contiene datos inválidos")
    {
        if (modelState.Keys.Any(key => key.StartsWith('$')))
        {
            return BadRequest(message, ["El JSON enviado no tiene un formato válido"]);
        }

        var errors = modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error =>
                string.IsNullOrWhiteSpace(error.ErrorMessage) || error.ErrorMessage.StartsWith("The value")
                    ? $"El valor del campo '{entry.Key}' no es válido."
                    : error.ErrorMessage))
            .ToList();

        return BadRequest(message, errors);
    }

    public static BaseResponse<T> NotFound(string message = "Recurso no encontrado")
    {
        return new BaseResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = HttpStatusCode.NotFound
        };
    }

    public static BaseResponse<T> Conflict(string message = "El recurso ya existe")
    {
        return new BaseResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = HttpStatusCode.Conflict
        };
    }

    public static BaseResponse<T> Unauthorized(string message = "No autorizado")
    {
        return new BaseResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = HttpStatusCode.Unauthorized
        };
    }
}