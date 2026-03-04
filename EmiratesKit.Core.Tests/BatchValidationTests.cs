using EmiratesKit.Core.Validators;
using FluentAssertions;
using Xunit;

namespace EmiratesKit.Core.Tests;

public class BatchValidationTests
{
    // ── Emirates ID batch ──────────────────────────────────────────

    [Fact]
    public void ParseMany_EmiratesId_Returns_Result_For_Each_Input()
    {
        var base14 = "78419901234567";
        var check  = EmiratesIdValidator.ComputeCheckDigit(base14);
        var validId = base14 + check;

        var inputs  = new[] { validId, "invalid", null };
        var results = EmiratesIdValidator.ParseMany(inputs);

        results.Should().HaveCount(3);
        results[0].IsValid.Should().BeTrue();
        results[0].Input.Should().Be(validId);
        results[1].IsValid.Should().BeFalse();
        results[2].IsValid.Should().BeFalse();
        results[2].ErrorCode.Should().Be("EMPTY_INPUT");
    }

    [Fact]
    public void ParseMany_EmiratesId_Processes_All_Items_Even_When_Some_Fail()
    {
        var inputs  = new[] { "bad1", "bad2", "bad3" };
        var results = EmiratesIdValidator.ParseMany(inputs);

        // Must return 3 results, not stop at first failure
        results.Should().HaveCount(3);
        results.All(r => !r.IsValid).Should().BeTrue();
    }

    [Fact]
    public void ParseMany_EmiratesId_Returns_Empty_List_For_Empty_Input()
    {
        var results = EmiratesIdValidator.ParseMany(Array.Empty<string>());
        results.Should().BeEmpty();
    }

    [Fact]
    public void ParseMany_EmiratesId_Preserves_Input_In_Result()
    {
        var inputs  = new[] { "784-1990-0000000-0" };
        var results = EmiratesIdValidator.ParseMany(inputs);

        results[0].Input.Should().Be("784-1990-0000000-0");
    }

    // ── IBAN batch ────────────────────────────────────────────────

    [Fact]
    public void ParseMany_Iban_Returns_Valid_And_Invalid_Results()
    {
        var inputs  = new[] { "AE070331234567890123456", "AE000000000000000000000" };
        var results = UaeIbanValidator.ParseMany(inputs);

        results.Should().HaveCount(2);
        results[0].IsValid.Should().BeTrue();
        results[1].IsValid.Should().BeFalse();
    }

    // ── Mobile batch ──────────────────────────────────────────────

    [Fact]
    public void ParseMany_Mobile_Handles_Mixed_Formats()
    {
        var inputs  = new[] { "+971501234567", "0551234567", "+971401234567" };
        var results = UaeMobileValidator.ParseMany(inputs);

        results.Should().HaveCount(3);
        results[0].IsValid.Should().BeTrue();
        results[1].IsValid.Should().BeTrue();
        results[2].IsValid.Should().BeFalse();
        results[2].ErrorCode.Should().Be("INVALID_PREFIX");
    }

    // ── TRN batch ─────────────────────────────────────────────────

    [Fact]
    public void ParseMany_Trn_Returns_Correct_Results()
    {
        var inputs  = new[] { "100123456700003", "200123456700003" };
        var results = UaeTrnValidator.ParseMany(inputs);

        results[0].IsValid.Should().BeTrue();
        results[1].IsValid.Should().BeFalse();
    }
}
