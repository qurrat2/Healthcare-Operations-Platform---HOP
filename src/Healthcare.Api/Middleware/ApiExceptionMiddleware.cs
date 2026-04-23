using System.Net;
using System.Text.Json;
using Healthcare.Application.Exceptions;
using Healthcare.Contracts.Common;

namespace Healthcare.Api.Middleware;

public sealed class ApiExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotImplementedException exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Fail(exception.Message);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (ApiException exception)
        {
            context.Response.StatusCode = (int)exception.StatusCode;
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = exception.Message,
                Errors = exception.Errors
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
