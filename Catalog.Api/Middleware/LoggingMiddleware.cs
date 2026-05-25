using System.Diagnostics;

namespace Catalog.Api.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        _logger.LogInformation("Request started: {Method} {Path} {RequestId}",
            request.Method, request.Path, context.TraceIdentifier);

        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();

        _logger.LogInformation("Request finished: {Method} {Path} {StatusCode} {ElapsedMs}ms {RequestId}",
            request.Method, request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds, context.TraceIdentifier);
    }
}
