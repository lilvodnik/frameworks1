using System.Diagnostics;

namespace Catalog.Api.Middleware;

public class TimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TimingMiddleware> _logger;

    public TimingMiddleware(RequestDelegate next, ILogger<TimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        context.Response.OnStarting(() =>
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            context.Response.Headers["X-Response-Time"] = $"{elapsedMs}ms";
            _logger.LogDebug("Response time {ElapsedMs}ms for {RequestId}",
                elapsedMs, context.TraceIdentifier);
            return Task.CompletedTask;
        });

        await _next(context);
    }
}