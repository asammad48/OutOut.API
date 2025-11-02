using OutOut.Constants.Enums;
using OutOut.Models.EntityInterfaces;
using System;

namespace OutOut.Models.Models
{
    public class Category : INonSqlEntity
    {
        public Category()
        {
            CreationDate = DateTime.UtcNow;
        }
        public DateTime CreationDate { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public TypeFor TypeFor { get; set; }
        public bool IsActive { get; set; }
        public int Order { get; set; }

    }
}
