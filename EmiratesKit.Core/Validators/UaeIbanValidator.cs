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
