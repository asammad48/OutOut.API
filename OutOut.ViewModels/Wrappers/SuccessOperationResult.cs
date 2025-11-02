using System.Collections.Generic;

namespace OutOut.ViewModels.Wrappers
{
    public class SuccessOperationResult<T> : OperationResult<T>
    {
        public SuccessOperationResult(T result)
        {
            Status = true;
            Result = result;
            Errors = new List<string>();
        }
    }
}
