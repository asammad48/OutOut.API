using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace OutOut.ViewModels.Validators
{
    public class FilesExtensionAttribute : ValidationAttribute
    {
        private readonly string[] _allowedExtensions;
        private readonly long _maxSize;

        public FilesExtensionAttribute(string[] allowedExtensions, long maxSize)
        {
            _allowedExtensions = allowedExtensions;
            _maxSize = maxSize;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is List<IFormFile> files)
            {
                if (!files.Any())
                    return ValidationResult.Success;

                foreach (var file in files.ToList())
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
                                continue;
                            }
                        }
                    }
                }
                return ValidationResult.Success;
            }
            return new ValidationResult("Invalid file type.");
        }
    }
}
