# EmiratesKit.FluentValidation

[![NuGet](https://img.shields.io/nuget/v/EmiratesKit.FluentValidation.svg)](https://www.nuget.org/packages/EmiratesKit.FluentValidation)
[![Build](https://github.com/akhilpvijayan/EmiratesKit/actions/workflows/build.yml/badge.svg)](https://github.com/akhilpvijayan/EmiratesKit/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

FluentValidation rule extensions for UAE documents. Adds `.ValidEmiratesId()`, `.ValidUaeIban()`, `.ValidUaeTrn()`, `.ValidUaeMobile()`, and `.ValidUaePassport()` extension methods to FluentValidation rule builders.

Depends on `EmiratesKit.Core` and `FluentValidation` 11.x, both installed automatically.

---

## Installation

```bash
dotnet add package EmiratesKit.FluentValidation
```

`EmiratesKit.Core` and `FluentValidation` are installed automatically as dependencies.

---

## When to Use This Package

Use `EmiratesKit.FluentValidation` when you are already using FluentValidation in your project and want UAE validation rules that integrate naturally into your existing validator classes.

Choose this over `EmiratesKit.Annotations` when you need:

- Conditional validation with `.When()` and `.Unless()`
- Cross-field rules that depend on other properties
- Custom error messages per rule without polluting your model
- Chaining UAE rules with other FluentValidation rules like `.NotEmpty()`, `.MaximumLength()`, or `.WithSeverity()`
- Testable validators through FluentValidation's own test helpers

---

## Available Extensions

All extensions are on `IRuleBuilder<T, string?>` and work with nullable string properties.

| Extension | Validates |
|---|---|
| `.ValidEmiratesId()` | Emirates ID format `784-YYYY-NNNNNNN-C` with Luhn check |
| `.ValidUaeIban()` | UAE IBAN starting with `AE`, 23 characters, Mod-97 check |
| `.ValidUaeTrn()` | 15-digit TRN starting with `100` |
| `.ValidUaeMobile()` | UAE mobile in any accepted format with carrier prefix check |
| `.ValidUaePassport()` | 1 uppercase letter followed by 7 digits |

---

## Basic Usage

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

---

## Custom Error Messages

Pass a custom message directly to the extension method:

```csharp
RuleFor(x => x.EmiratesId)
    .NotEmpty().WithMessage("Emirates ID is required.")
    .ValidEmiratesId("The Emirates ID you entered is not valid. Expected format: 784-YYYY-NNNNNNN-C.");

RuleFor(x => x.Mobile)
    .NotEmpty().WithMessage("Mobile number is required.")
    .ValidUaeMobile("Enter a valid UAE mobile number (e.g. 050 123 4567 or +971 50 123 4567).");

RuleFor(x => x.BankAccount)
    .ValidUaeIban("Bank account must be a valid UAE IBAN starting with AE.")
    .When(x => x.BankAccount is not null);
```

If no message is passed, a default message is used that includes the error code from the underlying validator (e.g. `INVALID_CHECKSUM`).

---

## Conditional Validation

Use `.When()` to apply a rule only under certain conditions:

```csharp
// Validate TRN only when the customer is VAT-registered
RuleFor(x => x.TaxRegistrationNumber)
    .NotEmpty().WithMessage("TRN is required for VAT-registered customers.")
    .ValidUaeTrn()
    .When(x => x.IsVatRegistered);

// Validate passport only when Emirates ID is not provided
RuleFor(x => x.PassportNumber)
    .NotEmpty().WithMessage("Either Emirates ID or Passport Number is required.")
    .ValidUaePassport()
    .When(x => string.IsNullOrEmpty(x.EmiratesId));

// Validate IBAN only when payment method is bank transfer
RuleFor(x => x.BankAccount)
    .NotEmpty().WithMessage("Bank account is required for bank transfer.")
    .ValidUaeIban()
    .When(x => x.PaymentMethod == "BankTransfer");
```

---

## Chaining with Other Rules

The extensions return the same `IRuleBuilder` so you can chain additional FluentValidation rules before or after:

```csharp
RuleFor(x => x.EmiratesId)
    .NotEmpty()
    .MaximumLength(20)       // runs before UAE validation
    .ValidEmiratesId()
    .WithSeverity(Severity.Error);

RuleFor(x => x.Mobile)
    .NotEmpty()
    .ValidUaeMobile()
    .WithName("Phone Number");   // changes the field name in error messages
```

---

## Registering with ASP.NET Core

Register your validators with the DI container using FluentValidation's built-in extension:

```csharp
// Program.cs
using FluentValidation;
using FluentValidation.AspNetCore;

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerValidator>();
```

With `AddFluentValidationAutoValidation()`, validation runs automatically on every request — the same behaviour as DataAnnotations, but using your FluentValidation validators. Controllers do not need to check `ModelState.IsValid`.

---

## Manual Validation

To validate manually outside of the request pipeline:

```csharp
var validator = new CreateCustomerValidator();
var result    = await validator.ValidateAsync(request);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
        // EmiratesId: The Emirates ID you entered is not valid.
    }
}
```

---

## Testing Your Validators

Use FluentValidation's `TestValidate` helper to write clean unit tests:

```csharp
using FluentValidation.TestHelper;

public class CreateCustomerValidatorTests
{
    private readonly CreateCustomerValidator _validator = new();

    [Fact]
    public void Should_Pass_With_Valid_Emirates_Id()
    {
        var model = new CreateCustomerRequest
        {
            EmiratesId = "784-1990-1234567-6",
            Mobile     = "+971501234567"
        };

        _validator.TestValidate(model).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Emirates_Id_Has_Wrong_Checksum()
    {
        var model = new CreateCustomerRequest
        {
            EmiratesId = "784-1990-0000000-0",
            Mobile     = "+971501234567"
        };

        _validator.TestValidate(model)
                  .ShouldHaveValidationErrorFor(x => x.EmiratesId);
    }

    [Fact]
    public void Should_Not_Validate_Iban_When_Null()
    {
        var model = new CreateCustomerRequest
        {
            EmiratesId  = "784-1990-1234567-6",
            Mobile      = "+971501234567",
            BankAccount = null
        };

        _validator.TestValidate(model)
                  .ShouldNotHaveValidationErrorFor(x => x.BankAccount);
    }

    [Fact]
    public void Should_Fail_With_Invalid_Iban()
    {
        var model = new CreateCustomerRequest
        {
            EmiratesId  = "784-1990-1234567-6",
            Mobile      = "+971501234567",
            BankAccount = "AE000000000000000000000"
        };

        _validator.TestValidate(model)
                  .ShouldHaveValidationErrorFor(x => x.BankAccount);
    }
}
```

---

## Null and Empty Behaviour

All extension methods treat `null` and empty string as passing — they do not add a validation error for missing values. This matches FluentValidation's own convention.

To require a value, chain `.NotEmpty()` before the UAE extension:

```csharp
// Required — fails if null, empty, or invalid
RuleFor(x => x.EmiratesId)
    .NotEmpty()
    .ValidEmiratesId();

// Optional — only validated when a value is provided
RuleFor(x => x.PassportNumber)
    .ValidUaePassport()
    .When(x => !string.IsNullOrEmpty(x.PassportNumber));
```

---

## Related Packages

| Package | Purpose |
|---|---|
| [EmiratesKit.Core](https://www.nuget.org/packages/EmiratesKit.Core) | Core validators, static API, dependency injection support |
| [EmiratesKit.Annotations](https://www.nuget.org/packages/EmiratesKit.Annotations) | DataAnnotation attributes for automatic model binding validation |

---

## License

MIT License. Copyright 2026 Akhil P Vijayan.