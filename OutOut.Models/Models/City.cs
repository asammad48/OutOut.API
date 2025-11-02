using OutOut.Models.EntityInterfaces;
using System;
using System.Collections.Generic;

namespace OutOut.Models.Models
{
    public class City : INonSqlEntity
    {
        public City()
        {
            IsActive = true;
            CreationDate = DateTime.UtcNow;
        }
        public DateTime CreationDate { get; set; }
        public string Name { get; set; }
        public List<string> Areas { get; set; }
        public bool IsActive { get; set; }
        public Country Country { get; set; }
    }
}
