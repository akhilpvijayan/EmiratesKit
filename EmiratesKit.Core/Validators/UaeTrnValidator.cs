using EmiratesKit.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmiratesKit.Core.Validators
{
    public class UaeTrnValidator
    {
        private const int TrnLength = 15;
        private const string TrnPrefix = "100";

        private static readonly UaeTrnValidator _instance = new();
        public static bool Check(string? input) => _instance.IsValid(input);
        public static ValidationResult Parse(string? input) => _instance.Validate(input);

        public static IReadOnlyList<BatchValidationResult<ValidationResult>> ParseMany(
            IEnumerable<string?> inputs)
        {
            return inputs
                .Select(input => new BatchValidationResult<ValidationResult>
                {
                    Input  = input,
                    Result = _instance.Validate(input)
                })
                .ToList();
        }

        /// <summary>
        /// Returns a masked TRN safe for logging.
        /// Preserves the first 3 digits (100 prefix) and last 3 digits.
        /// Example: 100123456700003  →  100*********003
        /// </summary>
        public static string Mask(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
         
            var trimmed = input.Trim();
         
            if (trimmed.Length != 15 || !trimmed.All(char.IsDigit))
                return trimmed;
         
            return trimmed[..3] + new string('*', 9) + trimmed[^3..];
        }
        public bool IsValid(string? input) => Validate(input).IsValid;

        public ValidationResult Validate(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return ValidationResult.Fail("TRN cannot be empty.", "EMPTY_INPUT");

            var n = input.Replace(" ", "").Replace("-", "").Trim();

            if (n.Length != TrnLength)
                return ValidationResult.Fail($"TRN must be {TrnLength} digits. Got {n.Length}.", "INVALID_LENGTH");

            if (!n.All(char.IsDigit))
                return ValidationResult.Fail("Digits only.", "INVALID_CHARACTERS");

            if (!n.StartsWith(TrnPrefix))
                return ValidationResult.Fail($"Must start with {TrnPrefix}.", "INVALID_PREFIX");

            return ValidationResult.Success();
        }
    }
}
