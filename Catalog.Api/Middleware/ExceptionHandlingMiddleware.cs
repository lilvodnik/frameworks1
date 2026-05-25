using System.Net;
using System.Text.Json;

namespace Catalog.Api.Middleware;

public class ExceptionHandlingMiddleware
{
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var requestId = context.TraceIdentifier;
        var response = context.Response;

        // Если ответ уже начался – логируем и выходим (но в Production такого не должно быть)
        if (response.HasStarted)
        {
            _logger.LogError(ex, "Ответ уже начат, невозможно отправить кастомную ошибку. RequestId: {RequestId}", requestId);
            return;
        }

        response.ContentType = "application/json";
        response.StatusCode = ex switch
        {
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            ArgumentException or InvalidOperationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var errorResponse = new
        {
            errorCode = ex switch
            {
                KeyNotFoundException => "NOT_FOUND",
                ArgumentException or InvalidOperationException => "BAD_REQUEST",
                _ => "INTERNAL_SERVER_ERROR"
            },
            message = ex.Message,
            requestId
        };

        _logger.LogError(ex, "Ошибка запроса {RequestId}: {Message}", requestId, ex.Message);
        await response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}