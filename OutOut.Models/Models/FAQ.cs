using OutOut.Models.EntityInterfaces;
using System;

namespace OutOut.Models.Models
{
    public class FAQ : INonSqlEntity
    {
        public FAQ()
        {
            CreationDate = DateTime.UtcNow;
        }
        public DateTime CreationDate { get; set; }
        public int QuestionNumber { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
