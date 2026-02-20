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
    public class UaeTrnAttribute : ValidationAttribute
    {
        public UaeTrnAttribute()
            => ErrorMessage = "The field {0} is not a valid UAE TRN.";
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is null) return ValidationResult.Success;
            var r = new UaeTrnValidator().Validate(value.ToString());
            if (r.IsValid) return ValidationResult.Success;
            return new ValidationResult(FormatErrorMessage(ctx.DisplayName),
                new[] { ctx.MemberName ?? ctx.DisplayName });
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class UaeMobileAttribute : ValidationAttribute
    {
        public UaeMobileAttribute()
            => ErrorMessage = "The field {0} is not a valid UAE mobile number.";
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is null) return ValidationResult.Success;
            var r = new UaeMobileValidator().Validate(value.ToString());
            if (r.IsValid) return ValidationResult.Success;
            return new ValidationResult(FormatErrorMessage(ctx.DisplayName),
                new[] { ctx.MemberName ?? ctx.DisplayName });
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class UaePassportAttribute : ValidationAttribute
    {
        public UaePassportAttribute()
            => ErrorMessage = "The field {0} is not a valid UAE passport number.";
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is null) return ValidationResult.Success;
            var r = new UaePassportValidator().Validate(value.ToString());
            if (r.IsValid) return ValidationResult.Success;
            return new ValidationResult(FormatErrorMessage(ctx.DisplayName),
                new[] { ctx.MemberName ?? ctx.DisplayName });
        }
    }
}
