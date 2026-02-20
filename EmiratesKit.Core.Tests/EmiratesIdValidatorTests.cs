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
    public class EmiratesIdValidatorTests
    {
        private readonly EmiratesIdValidator _v = new();

        [Fact]
        public void Should_Return_Correct_BirthYear()
        {
            var b14 = "784-1990-1234567-6";
            var r = _v.Validate(b14);
            r.IsValid.Should().BeTrue();
            r.BirthYear.Should().Be(1990);
        }

        [Fact]
        public void Should_Return_Correct_SequenceNumber()
        {
            var b14 = "784-1990-1234567-6";
            _v.Validate(b14).SequenceNumber.Should().Be("1234567");
        }

        [Fact]
        public void Should_Reject_Null()
        {
            var r = _v.Validate(null);
            r.IsValid.Should().BeFalse();
            r.ErrorCode.Should().Be("EMPTY_INPUT");
        }

        [Fact]
        public void Should_Accept_Formatted_With_Correct_Dashes()
        {
            var base14 = "784-1990-1234567-6";
            _v.IsValid(base14).Should().BeTrue();
        }

        [Fact]
        public void Should_Reject_Wrong_Dash_Positions()
        {
            // Dashes in wrong places — same digits, wrong format
            _v.Validate("7841-990-1234567-1").ErrorCode.Should().Be("INVALID_FORMAT");
            _v.Validate("784-19901234567-1").ErrorCode.Should().Be("INVALID_FORMAT");
            _v.Validate("784-1990-12345671").ErrorCode.Should().Be("INVALID_FORMAT");
        }

        [Fact]
        public void Should_Accept_Raw_Digits_Without_Dashes()
        {
            var base14 = "78419901234567";
            var id = base14 + EmiratesIdValidator.ComputeCheckDigit(base14);
            _v.IsValid(id).Should().BeTrue();
        }

        [Fact]
        public void ComputeCheckDigit_Should_Produce_Valid_Id()
        {
            var b14 = "78419901234567";
            var id = b14 + EmiratesIdValidator.ComputeCheckDigit(b14);
            _v.IsValid(id).Should().BeTrue();
        }

        [Fact]
        public void ComputeCheckDigit_Throws_For_WrongLength()
        {
            Action act = () => EmiratesIdValidator.ComputeCheckDigit("7841990123");
            act.Should().Throw<ArgumentException>();
        }
    }

}
