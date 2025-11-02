using System;
using System.Collections.Generic;
using System.Linq;

namespace OutOut.Models.Models
{
    public class UserOTP
    {
        public string HashedOTP { get; set; }
        public List<DateTime> RequestHistory { get; set; } = new List<DateTime>();
    }
}
