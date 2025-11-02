using Microsoft.AspNetCore.Authorization;

namespace OutOut.Helpers.Authorization
{
    public static class AuthorizationPolicyBuilderExtensions
    {
        public static AuthorizationPolicyBuilder AddRoleRequirement(this AuthorizationPolicyBuilder builder, params string[] allowedRoles)
        {
            return builder.AddRequirements(new RoleRequirement(allowedRoles));
        }
    }
}
