using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace OutOut.ViewModels.Wrappers
{
    public class FailedOperationResult<T> : OperationResult<T>
    {
        public FailedOperationResult(string errorMessage)
        {
            Status = false;
            ErrorCode = -1;
            ErrorMessage = errorMessage;
            Errors = new List<string>();
        }

        public FailedOperationResult(int errorCode, string errorMessage)
        {
            Status = false;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Errors = new List<string>();
        }

        public FailedOperationResult(int errorCode, string errorMessage, List<string> errors)
        {
            Status = false;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Errors = errors;
        }

        public FailedOperationResult(string errorMessage, IdentityResult identityResult)
        {
            Status = false;
            ErrorMessage = errorMessage;
            List<string> ErrorsList = new List<string>();
            foreach (var error in identityResult.Errors)
            {
                ErrorsList.Add(error.Description);
            }
            Errors = ErrorsList;
        }

        public override string ToString()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            return JsonSerializer.Serialize<OperationResult<T>>(this, options);
        }
    }
}
