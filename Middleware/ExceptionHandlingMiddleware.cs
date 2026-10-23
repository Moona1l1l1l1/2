using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using UniversityApi.Common;

namespace UniversityApi.Middleware;

/// <summary>
/// Централизованная обработка непредвиденных ошибок. Клиент получает только безопасный ответ в формате
/// ReturnResult, подробности (StackTrace, SQL и т.д.) попадают только в лог.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            // Нарушение уникальности / внешнего ключа в БД (например, гонка двух запросов) -> 409
            if (ex is DbUpdateException { InnerException: PostgresException pg } &&
                (pg.SqlState == PostgresErrorCodes.UniqueViolation || pg.SqlState == PostgresErrorCodes.ForeignKeyViolation))
            {
                _logger.LogWarning(ex, "Database constraint violation ({SqlState}). TraceId: {TraceId}",
                    pg.SqlState, context.TraceIdentifier);

                await WriteAsync(context, 409, ErrorCodes.Conflict,
                    "The operation conflicts with existing data");
                return;
            }

            _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);

            await WriteAsync(context, 500, ErrorCodes.InternalServerError, "An unexpected error occurred");
        }
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, string errorCode, string message)
    {
        var result = ReturnResult<object>.Fail(statusCode, errorCode, message);
        result.TraceId = context.TraceIdentifier;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(result, JsonOptions));
    }
}
