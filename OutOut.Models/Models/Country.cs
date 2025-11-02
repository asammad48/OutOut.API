using OutOut.Models.EntityInterfaces;

namespace OutOut.Models.Models
{
    public class Country : INonSqlEntity
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
    }
}
