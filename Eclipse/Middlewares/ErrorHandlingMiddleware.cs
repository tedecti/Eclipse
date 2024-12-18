using Eclipse.Exceptions;
using Minio.Exceptions;

namespace Eclipse.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new ApiResponse<object>
                { Message = ex.Message, Data = Array.Empty<object>() });
        }
        catch (UnauthorizedAccessException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new ApiResponse<object>
                { Message = "Unauthorized access", Data = Array.Empty<object>() });
        }
        catch (AlreadyExistsException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new ApiResponse<object>
                { Message = ex.Message, Data = Array.Empty<object>() });
        }
        catch (ForbiddenException ex)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ApiResponse<object>
                { Message = ex.Message, Data = Array.Empty<object>() });
        }
        // catch (Exception)
        // {
        //     context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        //     await context.Response.WriteAsJsonAsync(new ApiResponse<object>
        //         { Message = "An error occurred while processing your request", Data = Array.Empty<object>()});
        // }
    }
}