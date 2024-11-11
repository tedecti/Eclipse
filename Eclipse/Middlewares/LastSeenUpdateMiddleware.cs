using System.Security.Claims;
using Eclipse.Repositories.Interfaces;

namespace Eclipse.Middlewares;

public class LastSeenUpdateMiddleware
{
    private readonly RequestDelegate _next;

    public LastSeenUpdateMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userService)
    {
        if (context.User.Identity is { IsAuthenticated: true })
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                await userService.UpdateLastSeen(Guid.Parse(userId));
            }
        }

        await _next(context);
    }
}