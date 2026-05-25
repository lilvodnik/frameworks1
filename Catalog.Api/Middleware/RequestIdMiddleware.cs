using System.Diagnostics;

namespace Catalog.Api.Middleware;

public class RequestIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestIdMiddleware> _logger;

    public RequestIdMiddleware(RequestDelegate next, ILogger<RequestIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string? requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault();
        if (string.IsNullOrEmpty(requestId))
            requestId = Activity.Current?.Id ?? context.TraceIdentifier;

        context.TraceIdentifier = requestId;
        context.Response.Headers["X-Request-ID"] = requestId;

        using (_logger.BeginScope(new Dictionary<string, object> { ["RequestId"] = requestId }))
        {
            await _next(context);
        }
    }
}
