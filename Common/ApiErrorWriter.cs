using System.Text.Json;

namespace UniversityApi.Common;

/// <summary>Пишет ошибку в едином формате ReturnResult (используется JWT-событиями 401/403).</summary>
public static class ApiErrorWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task WriteAsync(HttpContext context, int statusCode, string errorCode, string message)
    {
        var result = ReturnResult<object>.Fail(statusCode, errorCode, message);
        result.TraceId = context.TraceIdentifier;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        if (statusCode == StatusCodes.Status401Unauthorized)
            context.Response.Headers["WWW-Authenticate"] = "Bearer";

        await context.Response.WriteAsync(JsonSerializer.Serialize(result, JsonOptions));
    }
}
