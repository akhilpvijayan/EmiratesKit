using EmiratesKit.Core.Constants;
using EmiratesKit.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmiratesKit.Core.Validators
{
    public class UaeMobileValidator
    {
        private static readonly UaeMobileValidator _instance = new();
        public static bool Check(string? input) => _instance.IsValid(input);
        public static MobileInfo Parse(string? input) => _instance.Validate(input);
        public bool IsValid(string? input) => Validate(input).IsValid;

        public MobileInfo Validate(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return MobileInfo.Fail("Mobile cannot be empty.", "EMPTY_INPUT");

            var n = input.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Trim();

            // Strip country code → get 9-digit local number
            string local;
            if (n.StartsWith("+971")) local = n[4..];
            else if (n.StartsWith("00971")) local = n[5..];
            else if (n.StartsWith("971") && n.Length == 12) local = n[3..];
            else if (n.StartsWith("0") && n.Length == 10) local = n[1..];
            else local = n;

            if (local.Length != 9)
                return MobileInfo.Fail("Must be 9 digits after country code.", "INVALID_LENGTH");

            if (!local.All(char.IsDigit))
                return MobileInfo.Fail("Digits only.", "INVALID_CHARACTERS");

            if (local[0] != '5')
                return MobileInfo.Fail("Must start with 5.", "INVALID_PREFIX");

            var prefix = "0" + local[..2];
            if (!UaeMobilePrefixes.IsValidPrefix(prefix))
                return MobileInfo.Fail(
                    $"'{prefix}' is not a valid UAE prefix.", "INVALID_MOBILE_PREFIX");

            return MobileInfo.SuccessResult(
                "+971" + local, prefix, UaeMobilePrefixes.GetCarrier(prefix)!);
        }
    }
}
