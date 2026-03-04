using EmiratesKit.Core.Validators;
using FluentAssertions;
using Xunit;

namespace EmiratesKit.Core.Tests;

public class MaskingTests
{
    // ── Emirates ID ───────────────────────────────────────────────

    [Fact]
    public void Mask_EmiratesId_Hides_BirthYear_And_Sequence()
    {
        var base14 = "78419901234567";
        var check  = EmiratesIdValidator.ComputeCheckDigit(base14);
        var id     = $"784-1990-1234567-{check}";

        var masked = EmiratesIdValidator.Mask(id.Replace("-", ""));

        masked.Should().StartWith("784-****-*******-");
        masked.Should().EndWith(check.ToString());
        masked.Should().NotContain("1990");
        masked.Should().NotContain("1234567");
    }

    [Fact]
    public void Mask_EmiratesId_Returns_Empty_For_Null()
    {
        EmiratesIdValidator.Mask(null).Should().BeEmpty();
    }

    [Fact]
    public void Mask_EmiratesId_Returns_Input_Unchanged_When_Unparseable()
    {
        EmiratesIdValidator.Mask("not-an-id").Should().Be("not-an-id");
    }

    // ── IBAN ──────────────────────────────────────────────────────

    [Fact]
    public void Mask_Iban_Preserves_Country_Check_Bank_And_Last5()
    {
        var masked = UaeIbanValidator.Mask("AE070331234567890123456");

        masked.Should().StartWith("AE07033");
        masked.Should().EndWith("23456");
        masked.Should().Contain("***********");
        masked.Should().HaveLength(23);
    }

    [Fact]
    public void Mask_Iban_Returns_Empty_For_Null()
    {
        UaeIbanValidator.Mask(null).Should().BeEmpty();
    }

    // ── TRN ───────────────────────────────────────────────────────

    [Fact]
    public void Mask_Trn_Preserves_Prefix_And_Last3()
    {
        var masked = UaeTrnValidator.Mask("100123456700003");

        masked.Should().StartWith("100");
        masked.Should().EndWith("003");
        masked.Should().HaveLength(15);
        masked.Should().NotContain("123456700");
    }

    // ── Mobile ────────────────────────────────────────────────────

    [Fact]
    public void Mask_Mobile_Hides_Middle_Digits()
    {
        var masked = UaeMobileValidator.Mask("+971501234567");

        masked.Should().StartWith("+9715");
        masked.Should().EndWith("567");
        masked.Should().Contain("****");
    }

    [Theory]
    [InlineData("+971501234567")]
    [InlineData("0501234567")]
    [InlineData("00971501234567")]
    public void Mask_Mobile_Accepts_All_Input_Formats(string input)
    {
        var masked = UaeMobileValidator.Mask(input);
        masked.Should().Contain("****");
    }

    // ── Passport ─────────────────────────────────────────────────

    [Fact]
    public void Mask_Passport_Preserves_Letter_And_Last3()
    {
        var masked = UaePassportValidator.Mask("A1234567");

        masked.Should().StartWith("A");
        masked.Should().EndWith("567");
        masked.Should().Contain("****");
        masked.Should().HaveLength(8);
    }

    [Fact]
    public void Mask_Passport_Returns_Empty_For_Null()
    {
        UaePassportValidator.Mask(null).Should().BeEmpty();
    }
}
