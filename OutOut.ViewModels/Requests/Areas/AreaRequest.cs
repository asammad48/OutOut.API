using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Areas
{
    public class AreaRequest
    {
        [MaxLength(50)]
        public string Area { get; set; }
    }
}
