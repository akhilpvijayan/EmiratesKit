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
