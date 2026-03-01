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

        public static IReadOnlyList<BatchValidationResult<MobileInfo>> ParseMany(
            IEnumerable<string?> inputs)
        {
            return inputs
                .Select(input => new BatchValidationResult<MobileInfo>
                {
                    Input  = input,
                    Result = _instance.Validate(input)
                })
                .ToList();
        }

        /// <summary>
        /// Returns a masked mobile number safe for logging.
        /// Normalises to +971 format first, then masks middle 4 digits.
        /// Example: +971501234567  →  +9715****567
        /// </summary>
        public static string Mask(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
         
            var result = _instance.Validate(input);
         
            if (!result.IsValid || result.NormalizedNumber is null)
                return input.Trim();
         
            // NormalizedNumber is always +971XXXXXXXXX (13 chars)
            // Keep: +9715 (first 6) and last 3
            // Mask: middle 4 digits
            var n = result.NormalizedNumber;
            return n[..6] + "****" + n[^3..];
        }

        /// <summary>
        /// Normalises a UAE mobile number to the canonical +971XXXXXXXXX format.
        /// Accepts +971, 00971, 05X, or 5X prefix formats.
        /// Returns the input unchanged if it cannot be normalised.
        /// Does not validate carrier prefix — use Parse() to validate.
        /// </summary>
        public static string Sanitize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
         
            var trimmed = input.Trim().Replace(" ", "").Replace("-", "");
         
            // +971XXXXXXXXX — already canonical
            if (trimmed.StartsWith("+971") && trimmed.Length == 13)
                return trimmed;
         
            // 00971XXXXXXXXX — strip 00, add +
            if (trimmed.StartsWith("00971") && trimmed.Length == 14)
                return "+" + trimmed[2..];
         
            // 05XXXXXXXXX — local format with leading 0
            if (trimmed.StartsWith("0") && trimmed.Length == 10)
                return "+971" + trimmed[1..];
         
            // 5XXXXXXXXX — bare local digits
            if (trimmed.StartsWith("5") && trimmed.Length == 9)
                return "+971" + trimmed;
         
            return input.Trim(); // unrecognised format — return trimmed
        }
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
