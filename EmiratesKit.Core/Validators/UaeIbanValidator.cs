using EmiratesKit.Core.Constants;
using EmiratesKit.Core.Interfaces;
using EmiratesKit.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EmiratesKit.Core.Validators
{
    public class UaeIbanValidator : IUaeIbanValidator
    {
        private const int UaeIbanLength = 23;
        private const string UaeCountryCode = "AE";

        private static readonly UaeIbanValidator _instance = new();
        public static bool Check(string? input) => _instance.IsValid(input);
        public static IbanInfo Parse(string? input) => _instance.Validate(input);

        public static IReadOnlyList<BatchValidationResult<IbanInfo>> ParseMany(
            IEnumerable<string?> inputs)
        {
            return inputs
                .Select(input => new BatchValidationResult<IbanInfo>
                {
                    Input  = input,
                    Result = _instance.Validate(input)
                })
                .ToList();
        }

        /// <summary>
        /// Returns a masked IBAN safe for logging. Preserves country code, check digits,
        /// bank code, and last 5 digits of account. Masks the middle account digits.
        /// Example: AE070331234567890123456  →  AE07033**********23456
        /// </summary>
        public static string Mask(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
         
            var trimmed = input.Trim().Replace(" ", "").ToUpperInvariant();
         
            // UAE IBAN: AE(2) + check(2) + bank(3) + account(16) = 23 chars
            if (trimmed.Length != 23 || !trimmed.StartsWith("AE"))
                return trimmed;
         
            // Keep: AE + check digits (4) + bank code (3) = first 7 chars
            // Mask: account digits 1-11 (positions 7-17)
            // Keep: last 5 digits of account (positions 18-22)
            var prefix  = trimmed[..7];        // AE07033
            var masked  = new string('*', 11); // 11 masked digits
            var suffix  = trimmed[^5..];       // last 5
         
            return prefix + masked + suffix;
        }
        public bool IsValid(string? input) => Validate(input).IsValid;

        public IbanInfo Validate(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return IbanInfo.Fail("IBAN cannot be empty.", "EMPTY_INPUT");

            var n = input.Replace(" ", "").ToUpperInvariant();

            if (!n.StartsWith(UaeCountryCode))
                return IbanInfo.Fail(
                    $"Must start with AE. Got: {n[..2]}.", "INVALID_COUNTRY_CODE");

            if (n.Length != UaeIbanLength)
                return IbanInfo.Fail(
                    $"UAE IBAN must be {UaeIbanLength} chars. Got {n.Length}.", "INVALID_LENGTH");

            if (!n[2..].All(char.IsDigit))
                return IbanInfo.Fail("Only digits after AE.", "INVALID_CHARACTERS");

            if (!ValidateMod97(n))
                return IbanInfo.Fail("Checksum invalid (Mod-97).", "INVALID_CHECKSUM");

            return IbanInfo.SuccessResult(
                rawIban: n, checkDigits: n.Substring(2, 2),
                bankCode: n.Substring(4, 3),
                bankName: UaeBankCodes.Resolve(n.Substring(4, 3)),
                accountNumber: n.Substring(7, 16));
        }

        // ── ISO 13616 Mod-97 ─────────────────────────────────────────
        private static bool ValidateMod97(string iban)
        {
            var rearranged = iban[4..] + iban[..4];
            var numeric = string.Concat(rearranged.Select(c =>
                char.IsLetter(c) ? (c - 'A' + 10).ToString() : c.ToString()));
            return BigInteger.Parse(numeric) % 97 == 1;
        }
    }
}
