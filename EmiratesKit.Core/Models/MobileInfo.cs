using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmiratesKit.Core.Models
{
    public class MobileInfo : ValidationResult
    {
        public string? NormalizedNumber { get; init; }  // +971501234567
        public string? Prefix { get; init; }  // 050
        public string? Carrier { get; init; }  // e& (Etisalat)

        public static MobileInfo SuccessResult(
            string normalized, string prefix, string carrier) =>
            new()
            {
                IsValid = true,
                NormalizedNumber = normalized,
                Prefix = prefix,
                Carrier = carrier
            };

        public static new MobileInfo Fail(string errorMessage, string errorCode) =>
            new() { IsValid = false, ErrorMessage = errorMessage, ErrorCode = errorCode };
    }
}
