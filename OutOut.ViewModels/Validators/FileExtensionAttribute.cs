using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace OutOut.ViewModels.Validators
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        private readonly string[] _allowedExtensions;
        private readonly long _maxSize;

        public FileExtensionAttribute(string[] allowedExtensions, long maxSize)
        {
            _allowedExtensions = allowedExtensions;
            _maxSize = maxSize;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is IFormFile file)
            {
                var fileExtension = Path.GetExtension(file.FileName);
                if (file != null)
                {
                    if (file.Length > _maxSize)
                        return new ValidationResult("File size too big.");

                    foreach (var extension in _allowedExtensions)
                    {
                        if (extension == fileExtension.ToLower())
                        {
                            return ValidationResult.Success;
                        }
                    }

                }

                return new ValidationResult("Invalid file type.");
            }

            return ValidationResult.Success;
        }
    }
}
