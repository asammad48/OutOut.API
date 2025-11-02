using Microsoft.AspNetCore.Identity;
using OutOut.Constants.Errors;
using System.Net;

namespace OutOut.Models.Exceptions
{
    public class OutOutException : Exception
    {
        public readonly List<string> Errors = new List<string>();
        public ErrorCodes Code { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public OutOutException(ErrorCodes errorCode) : base(errorCode.ToMessage())
        {
            HttpStatusCode = HttpStatusCode.BadRequest;
            Code = errorCode;
        }
        public OutOutException(ErrorCodes errorCode, HttpStatusCode httpStatusCode) : base(errorCode.ToMessage())
        {
            HttpStatusCode = httpStatusCode;
            Code = errorCode;
        }
        public OutOutException(ErrorCodes errorCode, string message) : base(message)
        {
            HttpStatusCode = HttpStatusCode.BadRequest;
            Code = errorCode;
        }
        public OutOutException(IdentityResult identityResult) : base(ErrorCodes.IdentityErrors.ToString())
        {
            HttpStatusCode = HttpStatusCode.BadRequest;
            Code = ErrorCodes.IdentityErrors;
            foreach (var error in identityResult.Errors)
            {
                Errors.Add(error.Description);
            }
        }
    }
}
