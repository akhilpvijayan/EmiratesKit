using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmiratesKit.Core.Models
{
    public class IbanInfo : ValidationResult
    {
        public string? RawIban { get; init; }
        public string? CountryCode { get; init; }
        public string? CheckDigits { get; init; }
        public string? BankCode { get; init; }
        public string? BankName { get; init; }
        public string? AccountNumber { get; init; }

        public static IbanInfo SuccessResult(
            string rawIban, string checkDigits, string bankCode,
            string? bankName, string accountNumber) =>
            new()
            {
                IsValid = true,
                RawIban = rawIban,
                CountryCode = "AE",
                CheckDigits = checkDigits,
                BankCode = bankCode,
                BankName = bankName,
                AccountNumber = accountNumber
            };

        public static new IbanInfo Fail(string errorMessage, string errorCode) =>
            new() { IsValid = false, ErrorMessage = errorMessage, ErrorCode = errorCode };
    }
}
