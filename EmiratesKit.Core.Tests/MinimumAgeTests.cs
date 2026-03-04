using EmiratesKit.Core.Validators;
using FluentAssertions;
using Xunit;

namespace EmiratesKit.Core.Tests;

public class MinimumAgeTests
{
    private static string MakeId(int birthYear)
    {
        // Helper: builds a valid Emirates ID with a given birth year
        var seq    = "1234567";
        var base14 = $"784{birthYear}{seq}";
        var check  = EmiratesIdValidator.ComputeCheckDigit(base14);
        return $"784-{birthYear}-{seq}-{check}";
    }

    [Fact]
    public void Returns_True_When_Person_Is_Clearly_Old_Enough()
    {
        // Born 40 years ago, minimum age 21 — clearly true
        var id     = MakeId(DateTime.Now.Year - 40);
        var result = EmiratesIdValidator.MeetsMinimumAge(id, 21);
        result.Should().BeTrue();
    }

    [Fact]
    public void Returns_False_When_Person_Is_Clearly_Too_Young()
    {
        // Born 10 years ago, minimum age 21 — clearly false
        var id     = MakeId(DateTime.Now.Year - 10);
        var result = EmiratesIdValidator.MeetsMinimumAge(id, 21);
        result.Should().BeFalse();
    }

    [Fact]
    public void Returns_Null_When_Birth_Year_Is_Exactly_Threshold_Year()
    {
        // Born exactly 21 years ago — cannot determine without full DOB
        var id     = MakeId(DateTime.Now.Year - 21);
        var result = EmiratesIdValidator.MeetsMinimumAge(id, 21);
        result.Should().BeNull();
    }

    [Fact]
    public void Returns_Null_For_Invalid_Emirates_Id()
    {
        var result = EmiratesIdValidator.MeetsMinimumAge("invalid", 21);
        result.Should().BeNull();
    }

    [Fact]
    public void Returns_Null_For_Null_Input()
    {
        var result = EmiratesIdValidator.MeetsMinimumAge(null, 21);
        result.Should().BeNull();
    }

    [Fact]
    public void Throws_For_Negative_Minimum_Age()
    {
        var id = MakeId(DateTime.Now.Year - 25);
        Action act = () => EmiratesIdValidator.MeetsMinimumAge(id, -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Returns_True_For_Minimum_Age_Zero()
    {
        // Any valid Emirates ID meets minimum age 0
        var id     = MakeId(DateTime.Now.Year - 5);
        var result = EmiratesIdValidator.MeetsMinimumAge(id, 0);
        result.Should().BeTrue();
    }
}
