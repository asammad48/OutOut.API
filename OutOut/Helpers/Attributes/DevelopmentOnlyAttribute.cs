using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OutOut.Constants.Errors;
using OutOut.Models.Exceptions;
using System;
using System.Net;

namespace OutOut.Helpers.Attributes
{
    public class DevelopmentOnlyAttribute : Attribute, IResourceFilter
    {
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var env = context.HttpContext.RequestServices.GetService<IWebHostEnvironment>();
            if (env.IsProduction())
            {
                throw new OutOutException(ErrorCodes.PageNotFound, HttpStatusCode.NotFound);
            }
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }
    }
}
