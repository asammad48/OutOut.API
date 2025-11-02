using System;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Wrappers
{
    public class PaginationRequest
    {
        public static readonly PaginationRequest Max = new PaginationRequest(0, int.MaxValue);

        [Range(0, int.MaxValue)]
        public int PageNumber { get; set; } = 0;

        [Range(1, int.MaxValue)]
        public int PageSize { get; set; } = 10;

        public PaginationRequest() { }
        public PaginationRequest(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
