using MongoDB.Bson;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Validators
{
    public class MongoIdAttribute : ValidationAttribute
    {
        public MongoIdAttribute() { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is string)
            {
                var id = value as string;

                if (!ObjectId.TryParse(id, out var x))
                    return new ValidationResult($"Invalid id in {validationContext.DisplayName}.");

                return ValidationResult.Success;
            }

            if (value is IEnumerable<string>)
            {
                var values = value as IEnumerable<string>;
                foreach (var id in values)
                {
                    bool parsed = ObjectId.TryParse(id, out ObjectId a);
                    if (parsed)
                        continue;
                    return new ValidationResult($"one or more invalid id in {validationContext.DisplayName}");
                }
                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid use of  MongoId attribute.");
        }
    }
}
