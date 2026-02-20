using EmiratesKit.Core.Validators;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EmiratesKit.Core.Tests
{
    public class UaeIbanValidatorTests
    {
        [Fact]
        public void Should_Reject_Null()
            => new UaeIbanValidator().Validate(null).ErrorCode.Should().Be("EMPTY_INPUT");

        [Fact]
        public void Should_Reject_Wrong_Length()
            => new UaeIbanValidator().Validate("AE0703312345").ErrorCode.Should().Be("INVALID_LENGTH");

        [Fact]
        public void Should_Reject_Wrong_CountryCode()
            => new UaeIbanValidator().Validate("GB07033123456789012345").ErrorCode.Should().Be("INVALID_COUNTRY_CODE");

        [Fact]
        public void Should_Reject_Invalid_Checksum()
            => new UaeIbanValidator().Validate("AE990331234567890123456").ErrorCode.Should().Be("INVALID_CHECKSUM");
    }

    public class UaeTrnValidatorTests
    {
        [Theory]
        [InlineData("100123456700003")]
        [InlineData("100000000000001")]
        public void Should_Accept_Valid(string t) => UaeTrnValidator.Check(t).Should().BeTrue();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("200123456700003")]
        [InlineData("10012345670")]
        public void Should_Reject_Invalid(string? t) => UaeTrnValidator.Check(t).Should().BeFalse();
    }

    public class UaeMobileValidatorTests
    {
        [Theory]
        [InlineData("+971501234567")]
        [InlineData("0501234567")]
        [InlineData("00971501234567")]
        [InlineData("+971551234567")]
        public void Should_Accept_Valid(string m) => UaeMobileValidator.Check(m).Should().BeTrue();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("+971401234567")]
        public void Should_Reject_Invalid(string? m) => UaeMobileValidator.Check(m).Should().BeFalse();

        [Fact]
        public void Should_Normalize_To_E164()
        {
            var r = UaeMobileValidator.Parse("0501234567");
            r.IsValid.Should().BeTrue();
            r.NormalizedNumber.Should().Be("+971501234567");
        }

        [Fact]
        public void Should_Detect_Etisalat()
            => UaeMobileValidator.Parse("+971501234567").Carrier.Should().Contain("Etisalat");

        [Fact]
        public void Should_Detect_du()
            => UaeMobileValidator.Parse("+971551234567").Carrier.Should().Be("du");
    }

    public class UaePassportValidatorTests
    {
        [Theory]
        [InlineData("A1234567")]
        [InlineData("Z9999999")]
        [InlineData("B0000000")]
        public void Should_Accept_Valid(string p) => UaePassportValidator.Check(p).Should().BeTrue();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("12345678")]
        [InlineData("AB123456")]
        [InlineData("A123456")]
        [InlineData("A123456789")]
        public void Should_Reject_Invalid(string? p) => UaePassportValidator.Check(p).Should().BeFalse();
    }
}
