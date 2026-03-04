# EmiratesKit.Core

[![NuGet](https://img.shields.io/nuget/v/EmiratesKit.Core.svg)](https://www.nuget.org/packages/EmiratesKit.Core)
[![Build](https://github.com/akhilpvijayan/EmiratesKit/actions/workflows/build.yml/badge.svg)](https://github.com/akhilpvijayan/EmiratesKit/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Core validation library for UAE documents. Validates Emirates ID, UAE IBAN, Tax Registration Number (TRN), mobile numbers, and passport numbers.

Zero external dependencies. Supports .NET 6, .NET 7, and .NET 8.

This is the foundation package of the EmiratesKit family. The other packages — `EmiratesKit.Annotations` and `EmiratesKit.FluentValidation` — depend on this one and install it automatically.

---

## Installation

```bash
dotnet add package EmiratesKit.Core
```

---

## Validators

### Emirates ID

Emirates ID numbers follow the format `784-YYYY-NNNNNNN-C` where `784` is the UAE country code, `YYYY` is the holder's birth year, `NNNNNNN` is a 7-digit sequence number, and `C` is a Luhn check digit.

Both formatted input with dashes and raw 15-digit input are accepted. When dashes are provided, their positions are validated strictly — dashes in the wrong positions are rejected with `INVALID_FORMAT`.

```csharp
using EmiratesKit.Core.Validators;

// Quick boolean check
bool isValid = EmiratesIdValidator.Check("784-1990-1234567-6");   // True

// Full result with parsed fields
var result = EmiratesIdValidator.Parse("784-1990-1234567-6");

Console.WriteLine(result.IsValid);           // True
Console.WriteLine(result.BirthYear);         // 1990
Console.WriteLine(result.ApproximateAge);    // 35
Console.WriteLine(result.SequenceNumber);    // 1234567
Console.WriteLine(result.CountryCode);       // 784
Console.WriteLine(result.CheckDigit);        // 6

// Raw digits (no dashes) are also accepted
EmiratesIdValidator.Check("784199012345676");   // True

// Invalid — wrong check digit
var bad = EmiratesIdValidator.Parse("784-1990-0000000-0");
Console.WriteLine(bad.IsValid);              // False
Console.WriteLine(bad.ErrorCode);            // INVALID_CHECKSUM
Console.WriteLine(bad.ErrorMessage);         // Emirates ID checksum is invalid (Luhn algorithm failed).

// Invalid — dashes in wrong positions
var wrongFormat = EmiratesIdValidator.Parse("7841-990-1234567-6");
Console.WriteLine(wrongFormat.ErrorCode);    // INVALID_FORMAT
```

**Validation rules applied:**

1. Input must not be null or empty
2. If dashes are present, format must be exactly `784-YYYY-NNNNNNN-C`
3. Must be exactly 15 digits after removing dashes
4. Must start with country code `784`
5. Birth year must be between 1900 and the current year
6. Check digit must pass the Luhn (Mod 10) algorithm

---

### UAE IBAN

UAE IBANs follow the ISO 13616 standard. The format is `AE` + 2 check digits + 3-digit bank code + 16-digit account number, totalling 23 characters. Validation uses the Mod-97 algorithm.

```csharp
using EmiratesKit.Core.Validators;

// Quick boolean check
bool isValid = UaeIbanValidator.Check("AE070331234567890123456");   // True

// Full result with bank details
var result = UaeIbanValidator.Parse("AE070331234567890123456");

Console.WriteLine(result.IsValid);           // True
Console.WriteLine(result.BankCode);          // 033
Console.WriteLine(result.BankName);          // Emirates NBD
Console.WriteLine(result.AccountNumber);     // 1234567890123456
Console.WriteLine(result.CheckDigits);       // 07

// Invalid — wrong check digits
var bad = UaeIbanValidator.Parse("AE990331234567890123456");
Console.WriteLine(bad.IsValid);              // False
Console.WriteLine(bad.ErrorCode);            // INVALID_CHECKSUM
```

**Supported bank codes:**

| Code | Bank |
|---|---|
| 033 | Emirates NBD |
| 035 | First Abu Dhabi Bank (FAB) |
| 030 | Abu Dhabi Commercial Bank (ADCB) |
| 020 | Mashreq Bank |
| 040 | Dubai Islamic Bank (DIB) |
| 023 | Commercial Bank of Dubai (CBD) |
| 025 | Abu Dhabi Islamic Bank (ADIB) |
| 016 | RAKBANK |
| 031 | HSBC UAE |
| 022 | Standard Chartered UAE |

IBANs with an unrecognised bank code still pass validation if the Mod-97 check succeeds — `BankName` will be `null` in that case.

---

### UAE Mobile

All common UAE mobile input formats are normalised and validated. The local number must have a valid UAE carrier prefix.

```csharp
using EmiratesKit.Core.Validators;

// All formats are accepted
UaeMobileValidator.Check("+971501234567");    // True  — international with +
UaeMobileValidator.Check("00971501234567");   // True  — international with 00
UaeMobileValidator.Check("0501234567");       // True  — local with leading 0
UaeMobileValidator.Check("501234567");        // True  — local digits only

// Full result
var result = UaeMobileValidator.Parse("+971501234567");

Console.WriteLine(result.IsValid);            // True
Console.WriteLine(result.NormalizedNumber);   // +971501234567
Console.WriteLine(result.Prefix);             // 050
Console.WriteLine(result.Carrier);            // e& (Etisalat)

// Invalid — unknown prefix
var bad = UaeMobileValidator.Parse("+971401234567");
Console.WriteLine(bad.IsValid);               // False
Console.WriteLine(bad.ErrorCode);             // INVALID_MOBILE_PREFIX
```

**Valid prefixes:**

| Prefix | Carrier |
|---|---|
| 050, 052, 054, 056, 057 | e& (Etisalat) |
| 055, 058, 059 | du |

---

### TRN (Tax Registration Number)

UAE TRNs are 15-digit numbers issued by the Federal Tax Authority. They must start with `100`.

```csharp
using EmiratesKit.Core.Validators;

UaeTrnValidator.Check("100123456700003");    // True
UaeTrnValidator.Check("200123456700003");    // False — does not start with 100
UaeTrnValidator.Check("10012345670000");     // False — only 14 digits
```

---

### Passport

UAE passport numbers consist of one uppercase letter followed by exactly 7 digits.

```csharp
using EmiratesKit.Core.Validators;

UaePassportValidator.Check("A1234567");    // True
UaePassportValidator.Check("12345678");    // False — no leading letter
UaePassportValidator.Check("AB123456");    // False — two letters
```

---

## Batch Validation

`ParseMany()` validates a collection of inputs in one call and returns every result including failures. It never stops on first error, so callers see all failures at once. Useful for bulk onboarding, file imports, and data migration pipelines.

Available on all five validator classes.

```csharp
using EmiratesKit.Core.Validators;

var ids = new[] { "784-1990-1234567-6", "784-1990-0000000-0", null };

var results = EmiratesIdValidator.ParseMany(ids);

foreach (var r in results)
{
    if (r.IsValid)
        Console.WriteLine($"{r.Input} — valid, born {r.Result.BirthYear}");
    else
        Console.WriteLine($"{r.Input} — {r.ErrorCode}");
}

// 784-1990-1234567-6 — valid, born 1990
// 784-1990-0000000-0 — INVALID_CHECKSUM
//  — EMPTY_INPUT
```

Each item in the returned `IReadOnlyList<BatchValidationResult<T>>` exposes:

| Property | Type | Description |
|---|---|---|
| `Input` | `string?` | The original input string as provided |
| `Result` | `T` | The full validation result including all parsed fields |
| `IsValid` | `bool` | Shortcut for `Result.IsValid` |
| `ErrorCode` | `string?` | Shortcut for `Result.ErrorCode` |

---

## Masking

`Mask()` returns a version of the document number safe for logging, error messages, and display. Enough of the value is preserved to identify the record — not enough to reconstruct the full number.

Available on all five validator classes.

```csharp
using EmiratesKit.Core.Validators;

EmiratesIdValidator.Mask("784199012345676");          // 784-****-*******-6
UaeIbanValidator.Mask("AE070331234567890123456");      // AE07033***********23456
UaeTrnValidator.Mask("100123456700003");               // 100*********003
UaeMobileValidator.Mask("+971501234567");              // +9715****567
UaePassportValidator.Mask("A1234567");                 // A****567
```

**Masking strategy per document:**

| Document | Preserved | Masked |
|---|---|---|
| Emirates ID | Country code `784`, check digit | Birth year, sequence number |
| IBAN | `AE` + check digits + bank code + last 5 account digits | Middle 11 account digits |
| TRN | First 3 digits (`100`), last 3 digits | Middle 9 digits |
| Mobile | Country code `+971` + first carrier digit, last 3 digits | Middle 4 digits |
| Passport | Letter prefix, last 3 digits | Middle 4 digits |

`Mask()` accepts all valid input formats. Mobile normalises to `+971` before masking regardless of input format. Returns an empty string for null input. Returns the input unchanged if it cannot be parsed.

---

## Sanitize

`Sanitize()` reformats a messy or inconsistently formatted input into the canonical display format without validating it. Use this to clean data before storing or displaying. To validate after sanitizing, pass the result to `Parse()`.

Available on `EmiratesIdValidator` and `UaeMobileValidator`.

```csharp
using EmiratesKit.Core.Validators;

// Emirates ID — raw digits, spaces, or already formatted — all produce the same output
EmiratesIdValidator.Sanitize("784199012345676");      // 784-1990-1234567-6
EmiratesIdValidator.Sanitize("784 1990 1234567 6");   // 784-1990-1234567-6
EmiratesIdValidator.Sanitize("784-1990-1234567-6");   // 784-1990-1234567-6

// Mobile — all UAE formats normalised to +971XXXXXXXXX
UaeMobileValidator.Sanitize("+971501234567");          // +971501234567
UaeMobileValidator.Sanitize("00971501234567");          // +971501234567
UaeMobileValidator.Sanitize("0501234567");              // +971501234567
UaeMobileValidator.Sanitize("501234567");               // +971501234567
```

Returns an empty string for null input. Returns the trimmed input unchanged if it cannot be reformatted. Does not throw.

---

## MeetsMinimumAge

`MeetsMinimumAge()` checks whether the Emirates ID holder likely meets a minimum age requirement based on the birth year in the ID.

Returns `bool?` rather than `bool` because Emirates ID contains only the birth year, not the full date of birth. For the exact threshold year, the method cannot determine with certainty whether the person has crossed their birthday yet.

```csharp
using EmiratesKit.Core.Validators;

// Born 1984 — checking minimum age 21 — clearly old enough
bool? result = EmiratesIdValidator.MeetsMinimumAge("784-1984-1234567-6", 21);
// result = true

// Born 2015 — checking minimum age 21 — clearly too young
bool? result = EmiratesIdValidator.MeetsMinimumAge("784-2015-1234567-6", 21);
// result = false

// Born exactly 21 years ago — cannot confirm without full date of birth
bool? result = EmiratesIdValidator.MeetsMinimumAge("784-2004-1234567-6", 21);
// result = null
```

**Return values:**

| Value | Meaning |
|---|---|
| `true` | Birth year is before the threshold year — person is definitely old enough |
| `false` | Birth year is after the threshold year — person is definitely too young |
| `null` | Birth year equals the threshold year — full date of birth required to confirm |
| `null` | The Emirates ID is invalid or null |

Always handle the `null` case explicitly. For age-restricted services, treat `null` as "further verification required."

```csharp
var meetsAge = EmiratesIdValidator.MeetsMinimumAge(emiratesId, minimumAge: 21);

switch (meetsAge)
{
    case true:
        // Definitely old enough — proceed
        break;
    case false:
        // Definitely too young — reject
        break;
    case null:
        // Cannot determine — request additional verification
        break;
}
```

Throws `ArgumentOutOfRangeException` if `minimumAge` is negative.

---

## Dependency Injection

Register all validators with the ASP.NET Core service container in `Program.cs`:

```csharp
using EmiratesKit.Core.Extensions;

builder.Services.AddUaeValidators();
```

Inject the validator interface into any service, controller, or handler:

```csharp
using EmiratesKit.Core.Interfaces;

public class CustomerService
{
    private readonly IEmiratesIdValidator _emiratesIdValidator;
    private readonly IUaeIbanValidator    _ibanValidator;

    public CustomerService(
        IEmiratesIdValidator emiratesIdValidator,
        IUaeIbanValidator    ibanValidator)
    {
        _emiratesIdValidator = emiratesIdValidator;
        _ibanValidator       = ibanValidator;
    }

    public bool Onboard(string emiratesId, string iban)
    {
        var eidResult  = _emiratesIdValidator.Validate(emiratesId);
        var ibanResult = _ibanValidator.Validate(iban);

        if (!eidResult.IsValid)
            throw new ValidationException(eidResult.ErrorMessage);

        if (!ibanResult.IsValid)
            throw new ValidationException(ibanResult.ErrorMessage);

        return true;
    }
}
```

Using interfaces instead of the static API (`EmiratesIdValidator.Check()`) allows you to mock validators in unit tests.

**Registered interfaces:**

| Interface | Implementation |
|---|---|
| `IEmiratesIdValidator` | `EmiratesIdValidator` |
| `IUaeIbanValidator` | `UaeIbanValidator` |
| `IUaeTrnValidator` | `UaeTrnValidator` |
| `IUaeMobileValidator` | `UaeMobileValidator` |
| `IUaePassportValidator` | `UaePassportValidator` |

---

## Error Codes

Every failed validation returns a machine-readable `ErrorCode` string on the result object alongside the human-readable `ErrorMessage`.

| Error Code | Meaning |
|---|---|
| `EMPTY_INPUT` | Input was null or empty. |
| `INVALID_FORMAT` | Dashes present but not in correct positions (`784-YYYY-NNNNNNN-C`). |
| `INVALID_LENGTH` | Incorrect number of digits or characters. |
| `INVALID_CHARACTERS` | Non-digit characters after normalization. |
| `INVALID_COUNTRY_CODE` | Does not start with `784` (Emirates ID) or `AE` (IBAN). |
| `INVALID_BIRTH_YEAR` | Year segment cannot be parsed as an integer. |
| `INVALID_BIRTH_YEAR_RANGE` | Year is before 1900 or in the future. |
| `INVALID_CHECKSUM` | Luhn check (Emirates ID) or Mod-97 check (IBAN) failed. |
| `INVALID_PREFIX` | TRN does not start with `100`. |
| `INVALID_MOBILE_PREFIX` | Prefix is not a registered UAE carrier prefix. |

---

## API Reference

Quick reference for every static method across all validators.

| Method | Available on | Returns |
|---|---|---|
| `Check(string?)` | All validators | `bool` |
| `Parse(string?)` | All validators | Typed result (`EmiratesIdInfo`, `IbanInfo`, etc.) |
| `ParseMany(IEnumerable<string?>)` | All validators | `IReadOnlyList<BatchValidationResult<T>>` |
| `Mask(string?)` | All validators | `string` |
| `Sanitize(string?)` | EmiratesId, Mobile | `string` |
| `MeetsMinimumAge(string?, int)` | EmiratesId only | `bool?` |

---

## Important Limitation

EmiratesKit.Core performs format and checksum validation only. It does not connect to any UAE government system, database, or API.

A result of `IsValid = true` means the document number is structurally correct and passes its mathematical check. It does not confirm that the document was ever issued, belongs to a specific person, or remains valid.

For official verification, use the appropriate government service:

- Emirates ID: UAE Identity and Citizenship Authority (ICA)
- TRN: Federal Tax Authority (FTA) taxpayer lookup portal
- Mobile number: Telecommunications and Digital Government Regulatory Authority (TDRA)

---

## Related Packages

| Package | Purpose |
|---|---|
| [EmiratesKit.Annotations](https://www.nuget.org/packages/EmiratesKit.Annotations) | `[EmiratesId]`, `[UaeIban]`, `[UaeTrn]`, `[UaeMobile]`, `[UaePassport]` DataAnnotation attributes |
| [EmiratesKit.FluentValidation](https://www.nuget.org/packages/EmiratesKit.FluentValidation) | `.ValidEmiratesId()`, `.ValidUaeIban()`, `.ValidUaeTrn()`, `.ValidUaeMobile()`, `.ValidUaePassport()` rule extensions |

---

## License

MIT License. Copyright 2026 Akhil P Vijayan.