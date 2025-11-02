using System.Collections.Generic;

namespace OutOut.ViewModels.Wrappers
{
    public abstract class OperationResult<T>
    {
        public bool Status { get; set; }
        public T Result { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> Errors { get; set; }
    }
}
