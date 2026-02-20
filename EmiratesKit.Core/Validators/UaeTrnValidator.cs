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
