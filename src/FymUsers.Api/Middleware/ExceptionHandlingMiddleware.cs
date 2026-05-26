using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace FymUsers.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _log;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (DomainException dex)
        {
            await WriteProblem(ctx, dex.StatusCode, dex.Title, dex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unhandled exception");
            await WriteProblem(ctx, (int)HttpStatusCode.InternalServerError, "Server error", "An unexpected error occurred.");
        }
    }

    private static Task WriteProblem(HttpContext ctx, int status, string title, string detail)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = ctx.Request.Path
        };
        return ctx.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}

public class DomainException : Exception
{
    public int StatusCode { get; }
    public string Title { get; }
    public DomainException(int statusCode, string title, string message) : base(message)
    {
        StatusCode = statusCode;
        Title = title;
    }

    public static DomainException NotFound(string what)      => new(404, "Not found", $"{what} was not found.");
    public static DomainException Conflict(string message)   => new(409, "Conflict", message);
    public static DomainException BadRequest(string message) => new(400, "Bad request", message);
    public static DomainException Unauthorized()             => new(401, "Unauthorized", "Invalid credentials.");
}
