using EmiratesKit.Core.Validators;

Console.WriteLine("=== EmiratesKit Demo ===");

// ── Emirates ID ───────────────────────────────────────────────────
Console.WriteLine("\n── Emirates ID ─────────────────────");

// Generate a valid ID: compute check digit from first 14 raw digits
var base14 = "784-1990-1234567-6";

var r = EmiratesIdValidator.Parse(base14);
Console.WriteLine($"Input:      {base14}");
Console.WriteLine($"Valid:      {r.IsValid}");
Console.WriteLine($"Birth Year: {r.BirthYear}");
Console.WriteLine($"Age:        {r.ApproximateAge}");

// Invalid ID — wrong check digit, format is structurally fine
var bad = EmiratesIdValidator.Parse("784-1990-0000000-0");
Console.WriteLine($"Invalid:    {bad.ErrorMessage}");   // INVALID_CHECKSUM

// ── UAE IBAN ──────────────────────────────────────────────────────
Console.WriteLine("\n── UAE IBAN ─────────────────────────");
var iban = UaeIbanValidator.Parse("AE070331234567890123456");
Console.WriteLine($"Valid: {iban.IsValid}");
if (iban.IsValid) Console.WriteLine($"Bank: {iban.BankName ?? "Unknown"}");
else Console.WriteLine($"Error: {iban.ErrorMessage}");

// ── UAE Mobile ────────────────────────────────────────────────────
Console.WriteLine("\n── UAE Mobile ───────────────────────");
string[] nums = { "+971555243432", "0551234567", "+971401234567" };
foreach (var num in nums)
{
    var m = UaeMobileValidator.Parse(num);
    Console.WriteLine(m.IsValid
        ? $"  {num} => {m.NormalizedNumber} ({m.Carrier})"
        : $"  {num} => INVALID: {m.ErrorCode}");
}

// ── UAE TRN ───────────────────────────────────────────────────────
Console.WriteLine("\n── UAE TRN ──────────────────────────");
Console.WriteLine($"100123456700003 => {UaeTrnValidator.Check("100123456700003")}");
Console.WriteLine($"200123456700003 => {UaeTrnValidator.Check("200123456700003")}");

// ── UAE Passport ──────────────────────────────────────────────────
Console.WriteLine("\n── UAE Passport ─────────────────────");
Console.WriteLine($"A1234567 => {UaePassportValidator.Check("A1234567")}");
Console.WriteLine($"12345678 => {UaePassportValidator.Check("12345678")}");