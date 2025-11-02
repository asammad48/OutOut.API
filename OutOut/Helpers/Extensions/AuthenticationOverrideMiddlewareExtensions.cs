using Microsoft.AspNetCore.Authentication;
using OutOut.Helpers.Middleware;

namespace OutOut.Helpers.Extensions
{
    public static class AuthenticationOverrideMiddlewareExtension
    {
        public static IApplicationBuilder UseAuthenticationOverride(this IApplicationBuilder app, string defaultScheme)
        {
            return app.UseMiddleware<AuthenticationOverrideMiddleware>(new AuthenticationOptions { DefaultScheme = defaultScheme });
        }
        public static IApplicationBuilder UseAuthenticationOverride(this IApplicationBuilder app, AuthenticationOptions authenticationOptionsOverride)
        {
            return app.UseMiddleware<AuthenticationOverrideMiddleware>(authenticationOptionsOverride);
        }
    }
}
