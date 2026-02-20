using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmiratesKit.Core.Constants
{
    public static class UaeMobilePrefixes
    {
        public static readonly Dictionary<string, string> Prefixes = new()
    {
        // e& (Etisalat) prefixes
        { "050", "e& (Etisalat)" }, { "052", "e& (Etisalat)" },
        { "054", "e& (Etisalat)" }, { "056", "e& (Etisalat)" },
        { "057", "e& (Etisalat)" },
        // du prefixes
        { "055", "du" }, { "058", "du" }, { "059", "du" },
    };

        public static string? GetCarrier(string prefix) =>
            Prefixes.TryGetValue(prefix, out var carrier) ? carrier : null;

        public static bool IsValidPrefix(string prefix) => Prefixes.ContainsKey(prefix);
    }
}
