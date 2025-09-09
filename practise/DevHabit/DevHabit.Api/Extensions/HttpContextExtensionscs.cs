using System.Security.Claims;

namespace DevHabit.Api.Extensions;

public static class HttpContextExtensionscs
{
    public static string? GetUserId(this HttpContext context)
    {
        return context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
