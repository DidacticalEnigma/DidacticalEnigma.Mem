using Microsoft.AspNetCore.Http;

namespace DidacticalEnigma.Mem.Extensions
{
    public static class ControllerExtensions
    {
        public static string? GetUserId(this HttpRequest request)
        {
            return request.HttpContext.User.Identity?.IsAuthenticated == true 
                ? request.HttpContext.User.Identity?.Name
                : null;
        }
    }
}