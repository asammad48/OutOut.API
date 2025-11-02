using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OutOut.Constants.Errors;
using OutOut.Models;
using OutOut.Models.Exceptions;
using OutOut.Models.Identity;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Providers;
using System.Net;
using System.Security.Claims;

namespace OutOut.API.Middleware
{
    public class UserDetailsMiddleware
    {
        private readonly RequestDelegate _next;
        public UserDetailsMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context,
                                      IUserDetailsProvider userDetailsProvider,
                                      UserManager<ApplicationUser> userManager,
                                      IUserRepository userRepository,
                                      IWebHostEnvironment environment,
                                      IOptions<AppSettings> appSettingsOptions)
        {
            var isAuthenticated = context.User.Identity.IsAuthenticated;
            if (!isAuthenticated && environment.IsDevelopment())
            {
                var superAdmin = await userManager.FindByEmailAsync(appSettingsOptions.Value.SuperAdminEmail);
                if (superAdmin != null)
                {
                    var superAdminRoles = await userManager.GetRolesAsync(superAdmin);
                    userDetailsProvider.Initialize(superAdmin, superAdminRoles.ToList(), string.Empty);
                    await userRepository.UpdateLastUsageDate(superAdmin.Id);
                }
                else
                {
                    userDetailsProvider.InitializeUnAuthenticated();
                }

                await _next(context);
                return;
            }
            else if (!isAuthenticated)
            {
                userDetailsProvider.InitializeUnAuthenticated();
                await _next(context);
                return;
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = context.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
                throw new OutOutException(ErrorCodes.Unauthorized, HttpStatusCode.Locked);

            if (string.IsNullOrEmpty(user.Email))
                throw new OutOutException(ErrorCodes.InvalidLogin, HttpStatusCode.Locked);

            var accessToken = context.Request.Headers[HeaderNames.Authorization][0].Split(" ")[1];

            userDetailsProvider.Initialize(user, userRoles, accessToken);

            await userRepository.UpdateLastUsageDate(user.Id);

            await _next(context);
            return;
        }
    }
}
