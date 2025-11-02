using Microsoft.AspNetCore.Diagnostics;
using OutOut.Models.Exceptions;
using OutOut.ViewModels.Wrappers;
using System.Net;

namespace OutOut.Helpers.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var response = new FailedOperationResult<object>(contextFeature.Error.Message).ToString();
                        if (contextFeature.Error is OutOutException outoutException)
                        {
                            context.Response.StatusCode = (int)outoutException.HttpStatusCode;
                            response = new FailedOperationResult<object>((int)outoutException.Code, outoutException.Message, outoutException.Errors).ToString();
                        }
                        await context.Response.WriteAsync(response);
                    }
                });
            });
        }
    }
}
