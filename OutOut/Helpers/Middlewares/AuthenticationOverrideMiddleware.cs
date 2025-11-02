using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace OutOut.Helpers.Middleware
{
    public class AuthenticationOverrideMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AuthenticationOptions _authenticationOptionsOverride;

        public AuthenticationOverrideMiddleware(RequestDelegate next, AuthenticationOptions authenticationOptionsOverride)
        {
            this._next = next;
            this._authenticationOptionsOverride = authenticationOptionsOverride;
        }
        public async Task Invoke(HttpContext context)
        {
            // Add overriden options to HttpContext
            context.Features.Set(this._authenticationOptionsOverride);
            await this._next(context);
        }
    }


    public class CustomAuthenticationSchemeProvider : AuthenticationSchemeProvider
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public CustomAuthenticationSchemeProvider(IOptions<AuthenticationOptions> options, IHttpContextAccessor contextAccessor) : base(options)
        {
            this._contextAccessor = contextAccessor;
        }

        // Retrieves overridden options from HttpContext
        private AuthenticationOptions GetOverrideOptions()
        {
            HttpContext context = this._contextAccessor.HttpContext;
            return context.Features.Get<AuthenticationOptions>();
        }
        public override Task<AuthenticationScheme> GetDefaultAuthenticateSchemeAsync()
        {
            AuthenticationOptions overrideOptions = this.GetOverrideOptions();
            string overridenScheme = overrideOptions?.DefaultAuthenticateScheme ?? overrideOptions?.DefaultScheme;
            if (overridenScheme != null)
                return this.GetSchemeAsync(overridenScheme);
            return base.GetDefaultAuthenticateSchemeAsync();
        }
        public override Task<AuthenticationScheme> GetDefaultChallengeSchemeAsync()
        {
            AuthenticationOptions overrideOptions = this.GetOverrideOptions();
            string overridenScheme = overrideOptions?.DefaultChallengeScheme ?? overrideOptions?.DefaultScheme;
            if (overridenScheme != null)
                return this.GetSchemeAsync(overridenScheme);
            return base.GetDefaultChallengeSchemeAsync();
        }
        public override Task<AuthenticationScheme> GetDefaultForbidSchemeAsync()
        {
            AuthenticationOptions overrideOptions = this.GetOverrideOptions();
            string overridenScheme = overrideOptions?.DefaultForbidScheme ?? overrideOptions?.DefaultScheme;
            if (overridenScheme != null)
                return this.GetSchemeAsync(overridenScheme);
            return base.GetDefaultForbidSchemeAsync();
        }
        public override Task<AuthenticationScheme> GetDefaultSignInSchemeAsync()
        {
            AuthenticationOptions overrideOptions = this.GetOverrideOptions();
            string overridenScheme = overrideOptions?.DefaultSignInScheme ?? overrideOptions?.DefaultScheme;
            if (overridenScheme != null)
                return this.GetSchemeAsync(overridenScheme);
            return base.GetDefaultSignInSchemeAsync();
        }
        public override Task<AuthenticationScheme> GetDefaultSignOutSchemeAsync()
        {
            AuthenticationOptions overrideOptions = this.GetOverrideOptions();
            string overridenScheme = overrideOptions?.DefaultSignOutScheme ?? overrideOptions?.DefaultScheme;
            if (overridenScheme != null)
                return this.GetSchemeAsync(overridenScheme);
            return base.GetDefaultSignOutSchemeAsync();
        }
    }
}
