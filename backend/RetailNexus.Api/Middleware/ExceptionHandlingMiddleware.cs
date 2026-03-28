using System.Net;
using System.Text.Json;

namespace RetailNexus.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, IHostEnvironment env, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _env = env;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var path = context.Request.Path;

        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            BadHttpRequestException => (HttpStatusCode.BadRequest, "リクエストの形式が正しくありません。"),
            _ => (HttpStatusCode.InternalServerError, "サーバーエラーが発生しました。")
        };

        // エラーログ出力
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "サーバーエラー: {Path}", path);
        }
        else
        {
            _logger.LogWarning("HTTPエラー {StatusCode}: {Message} Path: {Path}", (int)statusCode, exception.Message, path);
        }

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
