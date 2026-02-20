using EmiratesKit.Core.Validators;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmiratesKit.FluentValidation.Extensions
{
    public static class UaeValidationExtensions
    {
        public static IRuleBuilderOptions<T, string?> ValidEmiratesId<T>(
            this IRuleBuilder<T, string?> rb)
            => rb.Must(id => string.IsNullOrWhiteSpace(id) || EmiratesIdValidator.Check(id))
                 .WithMessage("'{PropertyName}' is not a valid UAE Emirates ID.")
                 .WithErrorCode("INVALID_EMIRATES_ID");

        // Overload with custom message
        public static IRuleBuilderOptions<T, string?> ValidEmiratesId<T>(
            this IRuleBuilder<T, string?> rb, string msg)
            => rb.Must(id => string.IsNullOrWhiteSpace(id) || EmiratesIdValidator.Check(id))
                 .WithMessage(msg).WithErrorCode("INVALID_EMIRATES_ID");

        public static IRuleBuilderOptions<T, string?> ValidUaeIban<T>(
            this IRuleBuilder<T, string?> rb)
            => rb.Must(x => string.IsNullOrWhiteSpace(x) || UaeIbanValidator.Check(x))
                 .WithMessage("'{PropertyName}' is not a valid UAE IBAN.")
                 .WithErrorCode("INVALID_UAE_IBAN");

        public static IRuleBuilderOptions<T, string?> ValidUaeTrn<T>(
            this IRuleBuilder<T, string?> rb)
            => rb.Must(x => string.IsNullOrWhiteSpace(x) || UaeTrnValidator.Check(x))
                 .WithMessage("'{PropertyName}' is not a valid UAE TRN.")
                 .WithErrorCode("INVALID_UAE_TRN");

        public static IRuleBuilderOptions<T, string?> ValidUaeMobile<T>(
            this IRuleBuilder<T, string?> rb)
            => rb.Must(x => string.IsNullOrWhiteSpace(x) || UaeMobileValidator.Check(x))
                 .WithMessage("'{PropertyName}' is not a valid UAE mobile.")
                 .WithErrorCode("INVALID_UAE_MOBILE");

        public static IRuleBuilderOptions<T, string?> ValidUaePassport<T>(
            this IRuleBuilder<T, string?> rb)
            => rb.Must(x => string.IsNullOrWhiteSpace(x) || UaePassportValidator.Check(x))
                 .WithMessage("'{PropertyName}' is not a valid UAE passport.")
                 .WithErrorCode("INVALID_UAE_PASSPORT");
    }
}
