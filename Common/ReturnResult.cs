using System.Text.Json.Serialization;

namespace UniversityApi.Common;

/// <summary>Единый формат ответа всех endpoints.</summary>
public class ReturnResult<T>
{
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public T? Result { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? TraceId { get; set; }

    /// <summary>Детали ошибок валидации по полям (только при 400).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? Errors { get; set; }

    public static ReturnResult<T> Ok(T result, int statusCode = 200) => new()
    {
        StatusCode = statusCode,
        IsSuccess = true,
        Result = result
    };

    public static ReturnResult<T> Created(T result) => Ok(result, 201);

    public static ReturnResult<T> Fail(int statusCode, string errorCode, string errorMessage,
        IDictionary<string, string[]>? errors = null) => new()
    {
        StatusCode = statusCode,
        IsSuccess = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage,
        Errors = errors
    };
}
