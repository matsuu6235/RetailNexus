using System.Net;
using System.Text.Json;

namespace RetailNexus.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _env = env;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            BadHttpRequestException => (HttpStatusCode.BadRequest, "リクエストの形式が正しくありません。"),
            _ => (HttpStatusCode.InternalServerError, "サーバーエラーが発生しました。")
        };

        var response = new Dictionary<string, string> { ["message"] = message };

        // 開発環境では例外の詳細を含める
        if (_env.IsDevelopment() && statusCode == HttpStatusCode.InternalServerError)
        {
            response["detail"] = exception.ToString();
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
