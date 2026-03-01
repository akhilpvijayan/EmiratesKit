using EmiratesKit.Core.Constants;
using EmiratesKit.Core.Interfaces;
using EmiratesKit.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmiratesKit.Core.Validators
{
    public class EmiratesIdValidator : IEmiratesIdValidator
    {
        private static readonly Regex FormattedRegex =
            new(EmiratesIdConstants.FormattedPattern, RegexOptions.Compiled);
        private static readonly Regex RawRegex =
            new(EmiratesIdConstants.RawPattern, RegexOptions.Compiled);

        // Singleton for static API
        private static readonly EmiratesIdValidator _instance = new();

        // ── Static API (no DI needed) ────────────────────────────────
        public static bool Check(string? input) => _instance.IsValid(input);
        public static EmiratesIdInfo Parse(string? input) => _instance.Validate(input);

        /// <summary>
        /// Validates a collection of Emirates IDs and returns each input paired with its result.
        /// Always processes all inputs — does not stop on first failure.
        /// </summary>
        public static IReadOnlyList<BatchValidationResult<EmiratesIdInfo>> ParseMany(
            IEnumerable<string?> inputs)
        {
            return inputs
                .Select(input => new BatchValidationResult<EmiratesIdInfo>
                {
                    Input  = input,
                    Result = _instance.Validate(input)
                })
                .ToList();
        }

        /// <summary>
        /// Returns a masked version of the Emirates ID safe for logging and display.
        /// Birth year and sequence number are masked. Country code and check digit are preserved.
        /// Returns the input unchanged if it cannot be parsed to a valid format.
        /// Example: 784-1990-1234567-6  →  784-****-*******-6
        /// </summary>
        public static string Mask(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
         
            var trimmed    = input.Trim();
            var normalized = trimmed.Replace("-", "").Replace(" ", "");
         
            if (normalized.Length != EmiratesIdConstants.TotalDigits
                || !normalized.All(char.IsDigit))
                return trimmed; // cannot parse — return as-is
         
            // Keep: 784 (country code) and last digit (check digit)
            // Mask: positions 3-6 (birth year) and 7-13 (sequence)
            return $"784-****-*******-{normalized[^1]}";
        }

        /// <summary>
        /// Reformats an Emirates ID into the canonical 784-YYYY-NNNNNNN-C format.
        /// Accepts raw digits, space-separated, or already formatted input.
        /// Returns the input unchanged if it cannot be reformatted (wrong length, non-digits).
        /// Does not validate — use Parse() to validate.
        /// </summary>
        public static string Sanitize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
         
            // Strip all dashes and spaces to get raw digits
            var digits = input.Trim().Replace("-", "").Replace(" ", "");
         
            if (digits.Length != EmiratesIdConstants.TotalDigits
                || !digits.All(char.IsDigit))
                return input.Trim(); // cannot reformat — return trimmed original
         
            // Reformat to 784-YYYY-NNNNNNN-C
            return $"{digits[..3]}-{digits[3..7]}-{digits[7..14]}-{digits[14]}";
        }

        /// <summary>
        /// Checks whether the Emirates ID holder likely meets a minimum age requirement.
        /// Returns true if the birth year indicates the person is definitely old enough.
        /// Returns false if the birth year indicates the person is definitely too young.
        /// Returns null if the birth year is the exact threshold year and certainty is
        /// not possible from year alone (full date of birth required for confirmation).
        /// Returns null also if the Emirates ID is invalid.
        /// </summary>
        public static bool? MeetsMinimumAge(string? input, int minimumAge)
        {
            if (minimumAge < 0)
                throw new ArgumentOutOfRangeException(nameof(minimumAge),
                    "Minimum age cannot be negative.");
         
            var result = _instance.Validate(input);
         
            if (!result.IsValid) return null;
         
            var currentYear    = DateTime.Now.Year;
            var thresholdYear  = currentYear - minimumAge;
         
            if (result.BirthYear < thresholdYear)  return true;   // definitely old enough
            if (result.BirthYear > thresholdYear)  return false;  // definitely too young
         
            // BirthYear == thresholdYear — depends on exact date of birth
            return null;
        }

        // ── Instance API (for DI / mocking) ─────────────────────────
        public bool IsValid(string? input) => Validate(input).IsValid;

        public EmiratesIdInfo Validate(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return EmiratesIdInfo.Fail("Emirates ID cannot be empty.", "EMPTY_INPUT");

            var trimmed = input.Trim();
            string normalized;

            if (trimmed.Contains('-'))
            {
                // Formatted input — enforce exact pattern 784-YYYY-NNNNNNN-C
                if (!FormattedRegex.IsMatch(trimmed))
                    return EmiratesIdInfo.Fail(
                        "Emirates ID must be in format 784-YYYY-NNNNNNN-C " +
                        "(e.g. 784-1990-1234567-1). Check dash positions and digit counts.",
                        "INVALID_FORMAT");

                normalized = trimmed.Replace("-", "");
            }
            else
            {
                // Raw input — 15 digits starting with 784
                if (trimmed.Length != EmiratesIdConstants.TotalDigits)
                    return EmiratesIdInfo.Fail(
                        $"Emirates ID must be {EmiratesIdConstants.TotalDigits} digits. Got {trimmed.Length}.",
                        "INVALID_LENGTH");

                if (!trimmed.All(char.IsDigit))
                    return EmiratesIdInfo.Fail(
                        "Emirates ID must contain digits only (or use format 784-YYYY-NNNNNNN-C).",
                        "INVALID_CHARACTERS");

                if (!RawRegex.IsMatch(trimmed))
                    return EmiratesIdInfo.Fail(
                        "Emirates ID must start with 784.",
                        "INVALID_COUNTRY_CODE");

                normalized = trimmed;
            }

            // From here normalized is always 15 clean digits
            var countryCode = normalized[..3];
            var birthYearStr = normalized.Substring(3, 4);

            if (!int.TryParse(birthYearStr, out var birthYear))
                return EmiratesIdInfo.Fail("Invalid birth year in Emirates ID.", "INVALID_BIRTH_YEAR");

            if (birthYear < EmiratesIdConstants.MinBirthYear || birthYear > DateTime.Now.Year)
                return EmiratesIdInfo.Fail(
                    $"Birth year {birthYear} is out of valid range " +
                    $"({EmiratesIdConstants.MinBirthYear}–{DateTime.Now.Year}).",
                    "INVALID_BIRTH_YEAR_RANGE");

            if (!ValidateLuhn(normalized))
                return EmiratesIdInfo.Fail(
                    "Emirates ID checksum is invalid (Luhn algorithm failed).",
                    "INVALID_CHECKSUM");

            return EmiratesIdInfo.SuccessResult(
                rawId: normalized,
                countryCode: countryCode,
                birthYear: birthYear,
                sequenceNumber: normalized.Substring(7, 7),
                checkDigit: normalized[^1] - '0'
            );
        }

        // ── Luhn Mod-10 ──────────────────────────────────────────────
        private static bool ValidateLuhn(string digits)
        {
            var sum = 0; var isDouble = false;
            for (var i = digits.Length - 1; i >= 0; i--)
            {
                var d = digits[i] - '0';
                if (isDouble) { d *= 2; if (d > 9) d -= 9; }
                sum += d; isDouble = !isDouble;
            }
            return sum % 10 == 0;
        }

        // ── Utility: compute check digit from first 15 digits ────────
        public static int ComputeCheckDigit(string first14Digits)
        {
            if (first14Digits.Length != 14 || !first14Digits.All(char.IsDigit))
                throw new ArgumentException("Must be exactly 14 digits.", nameof(first14Digits));

            var sum = 0;
            var isDouble = true;

            for (var i = first14Digits.Length - 1; i >= 0; i--)
            {
                var d = first14Digits[i] - '0';
                if (isDouble) { d *= 2; if (d > 9) d -= 9; }
                sum += d;
                isDouble = !isDouble;
            }

            return (10 - (sum % 10)) % 10;
        }
    }
}
