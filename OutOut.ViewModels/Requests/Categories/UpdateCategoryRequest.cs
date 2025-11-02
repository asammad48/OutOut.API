using Microsoft.AspNetCore.Http;
using OutOut.ViewModels.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Categories
{
    public class UpdateCategoryRequest
    {
        [MaxLength(100)]
        [Required]
        public string Name { get; set; }
        
        [IconFile]
        public IFormFile Icon { get; set; }
        
        [Required]
        public bool IsActive { get; set; }
    }
}
