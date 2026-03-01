using EmiratesKit.Core.Validators;
using FluentAssertions;
using Xunit;

namespace EmiratesKit.Core.Tests;

public class SanitizeTests
{
    // ── Emirates ID ───────────────────────────────────────────────

    [Theory]
    [InlineData("784199012345676",    "784-1990-1234567-6")]  // raw digits
    [InlineData("784 1990 1234567 6", "784-1990-1234567-6")]  // spaces
    [InlineData("784-1990-1234567-6", "784-1990-1234567-6")]  // already formatted
    public void Sanitize_EmiratesId_Formats_To_Canonical(string input, string expected)
    {
        EmiratesIdValidator.Sanitize(input).Should().Be(expected);
    }

    [Fact]
    public void Sanitize_EmiratesId_Returns_Empty_For_Null()
    {
        EmiratesIdValidator.Sanitize(null).Should().BeEmpty();
    }

    [Fact]
    public void Sanitize_EmiratesId_Returns_Input_For_Unparseable()
    {
        EmiratesIdValidator.Sanitize("not-valid").Should().Be("not-valid");
    }

    // ── Mobile ────────────────────────────────────────────────────

    [Theory]
    [InlineData("+971501234567",   "+971501234567")]  // already canonical
    [InlineData("00971501234567",  "+971501234567")]  // 00971 prefix
    [InlineData("0501234567",      "+971501234567")]  // local 0 prefix
    [InlineData("501234567",       "+971501234567")]  // bare digits
    public void Sanitize_Mobile_Normalises_To_Plus971_Format(string input, string expected)
    {
        UaeMobileValidator.Sanitize(input).Should().Be(expected);
    }

    [Fact]
    public void Sanitize_Mobile_Returns_Empty_For_Null()
    {
        UaeMobileValidator.Sanitize(null).Should().BeEmpty();
    }

    [Fact]
    public void Sanitize_Mobile_Returns_Input_For_Unrecognised_Format()
    {
        UaeMobileValidator.Sanitize("12345").Should().Be("12345");
    }
}
