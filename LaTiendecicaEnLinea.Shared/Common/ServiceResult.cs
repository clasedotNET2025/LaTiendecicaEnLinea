namespace LaTiendecicaEnLinea.Shared.Common;

public class ServiceResult
{
    public bool Succeded { get; set; }
    public string? Message { get; set; }
    public int StatusCode { get; set; } = 200;

    public static ServiceResult Success(string? message = null) => new()
    {
        Succeded = true,
        Message = message
    };

    public static ServiceResult Failure(string message, int statusCode = 500) => new()
    {
        Succeded = false,
        Message = message,
        StatusCode = statusCode
    };

    public static ServiceResult NotFound(string message) => new()
    {
        Succeded = false,
        Message = message,
        StatusCode = 404
    };

    public static ServiceResult ValidationError(string message) => new()
    {
        Succeded = false,
        Message = message,
        StatusCode = 400
    };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    public static ServiceResult<T> Success(T data, string? message = null) => new()
    {
        Succeded = true,
        Data = data,
        Message = message
    };

    public new static ServiceResult<T> Failure(string message, int statusCode = 500) => new()
    {
        Succeded = false,
        Message = message,
        StatusCode = statusCode
    };

    public new static ServiceResult<T> NotFound(string message) => new()
    {
        Succeded = false,
        Message = message,
        StatusCode = 404
    };

    public new static ServiceResult<T> ValidationError(string message) => new()
    {
        Succeded = false,
        Message = message,
        StatusCode = 400
    };
}