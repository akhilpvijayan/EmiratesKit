using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmiratesKit.Core.Models
{
    public class ValidationResult
    {
        public bool IsValid { get; init; }
        public string? ErrorMessage { get; init; }
        public string? ErrorCode { get; init; }

        public static ValidationResult Success() =>
            new() { IsValid = true };

        public static ValidationResult Fail(string errorMessage, string errorCode) =>
            new() { IsValid = false, ErrorMessage = errorMessage, ErrorCode = errorCode };
    }
}
