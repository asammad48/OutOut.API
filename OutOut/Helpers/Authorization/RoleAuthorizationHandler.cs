using Microsoft.AspNetCore.Authorization;
using OutOut.Persistence.Providers;

namespace OutOut.Helpers.Authorization
{
    public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
    {
        private readonly IUserDetailsProvider _userDetailsProvider;

        public RoleAuthorizationHandler(IUserDetailsProvider userDetailsProvider)
        {
            _userDetailsProvider = userDetailsProvider;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
        {
            if (context.User == null)
            {
                return Task.CompletedTask;
            }

            var roles = _userDetailsProvider.User.Roles;
            var hasRole = requirement.AllowedRoles.Any(allowedRole => roles.Contains(allowedRole));
            if (hasRole)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
