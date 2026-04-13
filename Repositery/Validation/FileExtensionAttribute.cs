using System;
using System.ComponentModel.DataAnnotations;

namespace Project_385.Repositery.Validation
{
    public class FileExtensionAttribute : ValidationAttribute

    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName);
                string[] extensions = { ".jpg", ".jpeg", ".png", ".gif" };
                bool result = extensions.Any(x => extension.EndsWith(x));
                if(!result)
                {
                    return new ValidationResult("Invalid file type. Only .jpg, .jpeg, .png, and .gif are allowed.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
