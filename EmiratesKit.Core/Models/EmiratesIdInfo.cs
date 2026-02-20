using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmiratesKit.Core.Models
{
    public class EmiratesIdInfo : ValidationResult
    {
        public string? RawId { get; init; }
        public string? CountryCode { get; init; }
        public int BirthYear { get; init; }
        public string? SequenceNumber { get; init; }
        public int CheckDigit { get; init; }

        public int ApproximateAge => BirthYear > 0
            ? DateTime.Now.Year - BirthYear : 0;

        public static EmiratesIdInfo SuccessResult(
            string rawId, string countryCode, int birthYear,
            string sequenceNumber, int checkDigit) =>
            new()
            {
                IsValid = true,
                RawId = rawId,
                CountryCode = countryCode,
                BirthYear = birthYear,
                SequenceNumber = sequenceNumber,
                CheckDigit = checkDigit
            };

        public static new EmiratesIdInfo Fail(string errorMessage, string errorCode) =>
            new() { IsValid = false, ErrorMessage = errorMessage, ErrorCode = errorCode };
    }
}
