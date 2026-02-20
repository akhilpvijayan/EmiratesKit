using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmiratesKit.Core.Constants
{
    /// <summary>
    /// Constants for UAE Emirates ID structure.
    /// Format: 784-YYYY-NNNNNNN-C  (15 digits total, dashes optional)
    /// </summary>
    public static class EmiratesIdConstants
    {
        public const string CountryCode = "784";
        public const int TotalDigits = 15;
        public const int FormattedLength = 19;
        public const int MinBirthYear = 1900;
        public const string FormattedPattern = @"^784-\d{4}-\d{7}-\d$";
        public const string RawPattern = @"^784\d{12}$";
    }

}
