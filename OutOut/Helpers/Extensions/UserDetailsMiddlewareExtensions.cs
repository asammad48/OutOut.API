using OutOut.API.Middleware;

namespace OutOut.Helpers.Extensions
{
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class UserDetailsMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserDetailsMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserDetailsMiddleware>();
        }
    }
}
