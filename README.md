# EmiratesKit

[![NuGet](https://img.shields.io/nuget/v/EmiratesKit.Core.svg)](https://www.nuget.org/packages/EmiratesKit.Core)
[![Build](https://github.com/YOURUSERNAME/EmiratesKit/actions/workflows/build.yml/badge.svg)](https://github.com/YOURUSERNAME/EmiratesKit/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

UAE document validation library for .NET. Validates Emirates ID, UAE IBAN, Tax Registration Number (TRN), mobile numbers, and passport numbers with zero external dependencies.

Supports .NET 6, .NET 7, and .NET 8.

---

## Packages

Three packages are available depending on your project's needs. Install only what you use.

| Package | Purpose | Install |
|---|---|---|
| `EmiratesKit.Core` | Core validators. No external dependencies. | `dotnet add package EmiratesKit.Core` |
| `EmiratesKit.Annotations` | DataAnnotation attributes for model validation. | `dotnet add package EmiratesKit.Annotations` |
| `EmiratesKit.FluentValidation` | FluentValidation rule extensions. | `dotnet add package EmiratesKit.FluentValidation` |

`EmiratesKit.Annotations` and `EmiratesKit.FluentValidation` both depend on `EmiratesKit.Core`, which is installed automatically.

---

## Validation Rules

Each validator enforces the following rules:

| Document | Format | Length | Algorithm |
|---|---|---|---|
| Emirates ID | `784-YYYY-NNNNNNN-C` | 15 digits | Luhn (Mod 10) |
| UAE IBAN | `AE` + 2 check digits + 3 bank code + 16 account | 23 characters | Mod-97 (ISO 13616) |
| TRN | Digits only, starts with `100` | 15 digits | Structure check |
| Mobile | `+971 5X XXXXXXX` — all UAE formats accepted | 9 local digits | Prefix lookup |
| Passport | One uppercase letter followed by 7 digits | 8 characters | Regex pattern |

Emirates ID accepts both the formatted input (`784-1990-1234567-6`) and raw digits (`784199012345676`). When dashes are present, their positions are validated strictly — dashes in the wrong place are rejected.

---

## Quick Start

### Emirates ID

```csharp
using EmiratesKit.Core.Validators;

// Quick check — returns true or false
bool isValid = EmiratesIdValidator.Check("784-1990-1234567-6");

// Full parse — returns detailed result
var result = EmiratesIdValidator.Parse("784-1990-1234567-6");

Console.WriteLine(result.IsValid);          // True
Console.WriteLine(result.BirthYear);        // 1990
Console.WriteLine(result.ApproximateAge);   // 35
Console.WriteLine(result.SequenceNumber);   // 1234567
Console.WriteLine(result.CountryCode);      // 784
Console.WriteLine(result.CheckDigit);       // 6

// Invalid input
var bad = EmiratesIdValidator.Parse("784-1990-0000000-0");
Console.WriteLine(bad.IsValid);             // False
Console.WriteLine(bad.ErrorCode);           // INVALID_CHECKSUM
Console.WriteLine(bad.ErrorMessage);        // Emirates ID checksum is invalid (Luhn algorithm failed).
```

All accepted input formats:

```csharp
EmiratesIdValidator.Check("784-1990-1234567-6");   // formatted with dashes
EmiratesIdValidator.Check("784199012345676");        // raw digits, no dashes
```

### UAE IBAN

```csharp
using EmiratesKit.Core.Validators;

var result = UaeIbanValidator.Parse("AE070331234567890123456");

Console.WriteLine(result.IsValid);          // True
Console.WriteLine(result.BankCode);         // 033
Console.WriteLine(result.BankName);         // Emirates NBD
Console.WriteLine(result.AccountNumber);    // 1234567890123456
Console.WriteLine(result.CheckDigits);      // 07
```

### UAE Mobile

```csharp
using EmiratesKit.Core.Validators;

// All of these are accepted
UaeMobileValidator.Check("+971501234567");    // True
UaeMobileValidator.Check("00971501234567");   // True
UaeMobileValidator.Check("0501234567");       // True
UaeMobileValidator.Check("501234567");        // True

// Full parse
var result = UaeMobileValidator.Parse("+971501234567");

Console.WriteLine(result.IsValid);            // True
Console.WriteLine(result.NormalizedNumber);   // +971501234567
Console.WriteLine(result.Prefix);             // 050
Console.WriteLine(result.Carrier);            // e& (Etisalat)
```

Valid prefixes: `050`, `052`, `054`, `056`, `057` (e& Etisalat) and `055`, `058`, `059` (du).

### TRN

```csharp
using EmiratesKit.Core.Validators;

bool valid = UaeTrnValidator.Check("100123456700003");   // True
bool invalid = UaeTrnValidator.Check("200123456700003"); // False — does not start with 100
```

### Passport

```csharp
using EmiratesKit.Core.Validators;

bool valid = UaePassportValidator.Check("A1234567");   // True
bool invalid = UaePassportValidator.Check("12345678"); // False — no leading letter
```

---

## DataAnnotations

Use `EmiratesKit.Annotations` to validate model properties automatically in ASP.NET Core MVC or Razor Pages. Validation runs during model binding and returns standard `400 Bad Request` responses when attributes fail.

```bash
dotnet add package EmiratesKit.Annotations
```

```csharp
using System.ComponentModel.DataAnnotations;
using EmiratesKit.Annotations.Attributes;

public class CreateCustomerRequest
{
    [Required]
    [EmiratesId(ErrorMessage = "Please enter a valid Emirates ID in the format 784-YYYY-NNNNNNN-C.")]
    public string EmiratesId { get; set; } = "";

    [Required]
    [UaeMobile(ErrorMessage = "Please enter a valid UAE mobile number.")]
    public string Mobile { get; set; } = "";

    [UaeIban]
    public string? BankAccount { get; set; }

    [UaeTrn]
    public string? TaxRegistrationNumber { get; set; }

    [UaePassport]
    public string? PassportNumber { get; set; }
}
```

Null and empty values pass the attribute checks — use `[Required]` separately if the field is mandatory.

---

## FluentValidation

Use `EmiratesKit.FluentValidation` to add UAE validation rules inside a `AbstractValidator<T>` class.

```bash
dotnet add package EmiratesKit.FluentValidation
```

```csharp
using FluentValidation;
using EmiratesKit.FluentValidation.Extensions;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.EmiratesId)
            .NotEmpty()
            .ValidEmiratesId();

        RuleFor(x => x.Mobile)
            .NotEmpty()
            .ValidUaeMobile();

        RuleFor(x => x.BankAccount)
            .ValidUaeIban()
            .When(x => x.BankAccount is not null);

        RuleFor(x => x.TaxRegistrationNumber)
            .ValidUaeTrn()
            .When(x => x.TaxRegistrationNumber is not null);

        RuleFor(x => x.PassportNumber)
            .ValidUaePassport()
            .When(x => x.PassportNumber is not null);
    }
}
```

A custom error message can be passed to any extension method:

```csharp
RuleFor(x => x.EmiratesId)
    .ValidEmiratesId("The Emirates ID you entered is not valid.");
```

---

## Dependency Injection

Register all validators with the ASP.NET Core DI container in a single call:

```csharp
// Program.cs
using EmiratesKit.Core.Extensions;

builder.Services.AddUaeValidators();
```

Inject the validator interface into any service or controller:

```csharp
using EmiratesKit.Core.Interfaces;

public class CustomerService
{
    private readonly IEmiratesIdValidator _emiratesIdValidator;

    public CustomerService(IEmiratesIdValidator emiratesIdValidator)
    {
        _emiratesIdValidator = emiratesIdValidator;
    }

    public bool RegisterCustomer(string emiratesId)
    {
        var result = _emiratesIdValidator.Validate(emiratesId);

        if (!result.IsValid)
            throw new ValidationException(result.ErrorMessage);

        // proceed with registration
        return true;
    }
}
```

Using the interface rather than the static API (`EmiratesIdValidator.Check()`) allows you to mock the validator in unit tests.

---

## Error Codes

Every failed validation returns a machine-readable `ErrorCode` alongside the human-readable `ErrorMessage`. Use error codes for programmatic handling or localisation.

| Error Code | Meaning |
|---|---|
| `EMPTY_INPUT` | The input was null or empty. |
| `INVALID_FORMAT` | Dashes present but not in the correct positions for `784-YYYY-NNNNNNN-C`. |
| `INVALID_LENGTH` | Incorrect number of digits or characters. |
| `INVALID_CHARACTERS` | Non-digit characters found after normalization. |
| `INVALID_COUNTRY_CODE` | Emirates ID does not start with `784`, or IBAN does not start with `AE`. |
| `INVALID_BIRTH_YEAR` | The year segment could not be parsed as an integer. |
| `INVALID_BIRTH_YEAR_RANGE` | The year is before 1900 or in the future. |
| `INVALID_CHECKSUM` | Luhn check (Emirates ID) or Mod-97 check (IBAN) failed. |
| `INVALID_PREFIX` | TRN does not start with `100`. |
| `INVALID_MOBILE_PREFIX` | Mobile prefix is not a registered UAE carrier prefix. |

```csharp
var result = EmiratesIdValidator.Parse("784-1990-0000000-0");

switch (result.ErrorCode)
{
    case "INVALID_CHECKSUM":
        // handle checksum error
        break;
    case "INVALID_BIRTH_YEAR_RANGE":
        // handle year error
        break;
}
```

---

## Important Limitation

EmiratesKit performs format and checksum validation only. It does not connect to any UAE government system, database, or API.

A result of `IsValid = true` means the document number is structurally correct and passes its algorithm check. It does not confirm that the document exists, belongs to a real person, or has not been revoked.

For identity verification, KYC, or AML purposes, use the appropriate government gateway:

- Emirates ID verification: UAE Identity and Citizenship Authority (ICA)
- TRN verification: Federal Tax Authority (FTA) portal
- Mobile number ownership: Telecommunications and Digital Government Regulatory Authority (TDRA)

---

## Contributing

Pull requests are welcome. Please open an issue first to discuss the change you want to make. All PRs must target the `develop` branch and pass all CI checks before review.

---

## License

MIT License. Copyright 2026 Akhil P Vijayan.

See [LICENSE](LICENSE) for the full text.
