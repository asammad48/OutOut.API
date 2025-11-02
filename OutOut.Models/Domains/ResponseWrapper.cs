using System.Collections.Generic;

namespace OutOut.Models.Domains
{
    public class ResponseWrapper<T>
    {
        public bool Status { get; set; }
        public T Result { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> Errors { get; set; }
    }
}
