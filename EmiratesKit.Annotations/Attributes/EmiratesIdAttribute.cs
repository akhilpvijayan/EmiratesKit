using EmiratesKit.Core.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmiratesKit.Annotations.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class EmiratesIdAttribute : ValidationAttribute
    {
        public EmiratesIdAttribute()
            => ErrorMessage = "The field {0} is not a valid UAE Emirates ID.";

        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is null) return ValidationResult.Success;
            var result = new EmiratesIdValidator().Validate(value.ToString());
            if (result.IsValid) return ValidationResult.Success;
            return new ValidationResult(
                FormatErrorMessage(ctx.DisplayName),
                new[] { ctx.MemberName ?? ctx.DisplayName });
        }
    }
}
