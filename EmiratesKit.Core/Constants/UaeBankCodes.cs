using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmiratesKit.Core.Constants
{
    public static class UaeBankCodes
    {
        public static readonly Dictionary<string, string> Banks = new()
    {
        { "033", "Emirates NBD" },
        { "035", "First Abu Dhabi Bank (FAB)" },
        { "030", "Abu Dhabi Commercial Bank (ADCB)" },
        { "020", "Mashreq Bank" },
        { "040", "Dubai Islamic Bank (DIB)" },
        { "023", "Commercial Bank of Dubai (CBD)" },
        { "032", "Emirates Islamic Bank" },
        { "025", "Abu Dhabi Islamic Bank (ADIB)" },
        { "045", "Sharjah Islamic Bank" },
        { "065", "Ajman Bank" },
        { "060", "United Arab Bank (UAB)" },
        { "010", "National Bank of Fujairah (NBF)" },
        { "012", "Invest Bank" },
        { "016", "National Bank of Ras Al-Khaimah (RAKBANK)" },
        { "046", "Bank of Sharjah" },
        { "057", "Citibank UAE" },
        { "031", "HSBC UAE" },
        { "022", "Standard Chartered UAE" },
        { "048", "Lloyds Bank UAE" },
        { "052", "Barclays UAE" }
    };

        public static string? Resolve(string bankCode) =>
            Banks.TryGetValue(bankCode, out var name) ? name : null;

        public static bool IsKnown(string bankCode) => Banks.ContainsKey(bankCode);
    }
}
