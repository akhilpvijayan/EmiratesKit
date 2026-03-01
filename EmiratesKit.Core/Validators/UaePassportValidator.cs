using EmiratesKit.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmiratesKit.Core.Validators
{
    public class UaePassportValidator
    {
        private static readonly Regex Rx = new(@"^[A-Z]\d{7}$", RegexOptions.Compiled);
        private static readonly UaePassportValidator _instance = new();

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
        /// Returns a masked passport number safe for logging.
        /// Preserves the letter prefix and last 3 digits.
        /// Example: A1234567  →  A****567
        /// </summary>
        public static string Mask(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
         
            var trimmed = input.Trim().ToUpperInvariant();
         
            // Passport: 1 letter + 7 digits = 8 chars
            if (trimmed.Length != 8
                || !char.IsLetter(trimmed[0])
                || !trimmed[1..].All(char.IsDigit))
                return trimmed;
         
            return trimmed[0] + new string('*', 4) + trimmed[^3..];
        }
        public bool IsValid(string? input) => Validate(input).IsValid;

        public ValidationResult Validate(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return ValidationResult.Fail("Passport cannot be empty.", "EMPTY_INPUT");

            var n = input.Replace(" ", "").ToUpperInvariant().Trim();

            if (n.Length != 8)
                return ValidationResult.Fail($"Must be 8 chars. Got {n.Length}.", "INVALID_LENGTH");

            if (!Rx.IsMatch(n))
                return ValidationResult.Fail("Format: 1 letter + 7 digits (A1234567).", "INVALID_FORMAT");

            return ValidationResult.Success();
        }
    }
}
