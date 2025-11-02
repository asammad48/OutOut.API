using OutOut.Models.EntityInterfaces;
using System;

namespace OutOut.Models.Models
{
    public class TermsAndConditions : INonSqlEntity
    {
        public TermsAndConditions()
        {
            CreationDate = DateTime.UtcNow;
        }
        public DateTime CreationDate { get; set; }
        public string TermCondition { get; set; }
        public bool IsActive { get; set; }
    }
}
