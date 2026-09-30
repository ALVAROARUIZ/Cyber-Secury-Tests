using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
namespace Wallabee.API.Configuration;
public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var status = exception switch {
            FileNotFoundException or DirectoryNotFoundException or KeyNotFoundException => 404,
            UnauthorizedAccessException => 403,
            ArgumentException or InvalidOperationException => 400,
            IOException => 409,
            _ => 500 };
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = status == 500 ? "Internal server error" : "Request could not be completed" }, ct);
        return true;
    }
}
