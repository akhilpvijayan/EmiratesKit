# EmiratesKit.Annotations

[![NuGet](https://img.shields.io/nuget/v/EmiratesKit.Annotations.svg)](https://www.nuget.org/packages/EmiratesKit.Annotations)
[![Build](https://github.com/akhilpvijayan/EmiratesKit/actions/workflows/build.yml/badge.svg)](https://github.com/akhilpvijayan/EmiratesKit/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

DataAnnotation validation attributes for UAE documents. Decorates model properties with `[EmiratesId]`, `[UaeIban]`, `[UaeTrn]`, `[UaeMobile]`, and `[UaePassport]` to trigger automatic validation during ASP.NET Core model binding.

Depends on `EmiratesKit.Core`, which is installed automatically.

---

## Installation

```bash
dotnet add package EmiratesKit.Annotations
```

`EmiratesKit.Core` is installed automatically as a dependency. You do not need to install it separately.

---

## When to Use This Package

Use `EmiratesKit.Annotations` when you want validation to run automatically as part of ASP.NET Core model binding — without writing any validation logic in your controllers or services.

When a request arrives, ASP.NET Core binds the request body to your model, runs all validation attributes, and populates `ModelState`. If any attribute fails, `[ApiController]` returns a `400 Bad Request` response automatically before your action method is ever called.

If you need more control — conditional validation, cross-field rules, or custom error messages per rule — use `EmiratesKit.FluentValidation` instead.

---

## Available Attributes

| Attribute | Validates | Backed by |
|---|---|---|
| `[EmiratesId]` | Emirates ID in format `784-YYYY-NNNNNNN-C` | Luhn algorithm |
| `[UaeIban]` | UAE IBAN starting with `AE`, 23 characters | Mod-97 algorithm |
| `[UaeTrn]` | 15-digit TRN starting with `100` | Structure check |
| `[UaeMobile]` | UAE mobile in any accepted format | Prefix lookup |
| `[UaePassport]` | 1 uppercase letter + 7 digits | Regex pattern |

---

## Basic Usage

Decorate your model properties with the relevant attribute:

```csharp
using System.ComponentModel.DataAnnotations;
using EmiratesKit.Annotations.Attributes;

public class CreateCustomerRequest
{
    [Required]
    public string FullName { get; set; } = "";

    [Required]
    [EmiratesId]
    public string EmiratesId { get; set; } = "";

    [Required]
    [UaeMobile]
    public string Mobile { get; set; } = "";

    [UaeIban]
    public string? BankAccount { get; set; }

    [UaeTrn]
    public string? TaxRegistrationNumber { get; set; }

    [UaePassport]
    public string? PassportNumber { get; set; }
}
```

No changes are needed in the controller. With `[ApiController]`, validation runs automatically:

```csharp
[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] CreateCustomerRequest request)
    {
        // If EmiratesId or Mobile failed validation,
        // ASP.NET Core already returned 400 before reaching here.
        return Ok();
    }
}
```

---

## Custom Error Messages

Every attribute accepts a custom `ErrorMessage`:

```csharp
[EmiratesId(ErrorMessage = "Emirates ID must be in the format 784-YYYY-NNNNNNN-C.")]
public string EmiratesId { get; set; } = "";

[UaeMobile(ErrorMessage = "Please enter a valid UAE mobile number (e.g. 050 123 4567).")]
public string Mobile { get; set; } = "";

[UaeIban(ErrorMessage = "Bank account must be a valid UAE IBAN starting with AE.")]
public string? BankAccount { get; set; }

[UaeTrn(ErrorMessage = "Tax Registration Number must be 15 digits starting with 100.")]
public string? TaxRegistrationNumber { get; set; }

[UaePassport(ErrorMessage = "Passport number must be one letter followed by 7 digits.")]
public string? PassportNumber { get; set; }
```

---

## Null and Empty Behaviour

All attributes in this package treat `null` and empty string as passing. They only validate when a value is present.

This is consistent with how standard .NET attributes like `[EmailAddress]` and `[Phone]` behave.

To make a field required, combine the attribute with `[Required]`:

```csharp
// Optional — only validated when a value is provided
[UaeIban]
public string? BankAccount { get; set; }

// Required — must be present and must be a valid Emirates ID
[Required]
[EmiratesId]
public string EmiratesId { get; set; } = "";
```

---

## Validation Response Format

When a request fails model validation, ASP.NET Core returns a `400 Bad Request` with a `ValidationProblemDetails` body:

```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "EmiratesId": [
            "Emirates ID must be in the format 784-YYYY-NNNNNNN-C."
        ],
        "Mobile": [
            "Please enter a valid UAE mobile number."
        ]
    }
}
```

The key in `errors` is the property name. The value is the list of error messages from the failing attributes.

---

## Using with MVC and Razor Pages

The attributes work in all ASP.NET Core hosting models, not just Web API. For MVC controllers and Razor Pages, check `ModelState.IsValid` manually:

```csharp
// MVC controller
[HttpPost]
public IActionResult Create(CreateCustomerRequest request)
{
    if (!ModelState.IsValid)
        return View(request);

    // proceed
    return RedirectToAction("Index");
}
```

```csharp
// Razor Page
public IActionResult OnPost()
{
    if (!ModelState.IsValid)
        return Page();

    // proceed
    return RedirectToPage("Success");
}
```

---

## Using with Manual Validation

If you are not using ASP.NET Core model binding, you can trigger attribute validation manually using `Validator.TryValidateObject`:

```csharp
using System.ComponentModel.DataAnnotations;

var request = new CreateCustomerRequest
{
    EmiratesId = "784-1990-0000000-0",   // invalid checksum
    Mobile     = "+971501234567"
};

var context = new ValidationContext(request);
var results = new List<ValidationResult>();
bool isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

foreach (var result in results)
{
    Console.WriteLine(result.ErrorMessage);
    // Emirates ID checksum is invalid (Luhn algorithm failed).
}
```

---

## Related Packages

| Package | Purpose |
|---|---|
| [EmiratesKit.Core](https://www.nuget.org/packages/EmiratesKit.Core) | Core validators, static API, dependency injection support |
| [EmiratesKit.FluentValidation](https://www.nuget.org/packages/EmiratesKit.FluentValidation) | FluentValidation rule extensions for complex validation scenarios |

---

## License

MIT License. Copyright 2026 Akhil P Vijayan.