using FluentAssertions;
using SafeFlow.SharedKernel.Exceptions;
using SafeFlow.SharedKernel.ValueObjects;
using Xunit;

namespace SafeFlow.Domain.Tests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="Email"/> value object.
/// </summary>
public sealed class EmailValueObjectTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("UPPER@EXAMPLE.COM")]
    [InlineData("user+tag@domain.org")]
    public void Create_WithValidEmail_ReturnsNormalisedLowercase(string input)
    {
        var email = Email.Create(input);

        email.Value.Should().Be(input.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    [InlineData("@missinglocal.com")]
    public void Create_WithInvalidEmail_ThrowsValidationException(string input)
    {
        var act = () => Email.Create(input);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_WithEmailExceedingMaxLength_ThrowsValidationException()
    {
        // 255 chars > RFC 5321 max of 254
        var tooLong = new string('a', 244) + "@example.com";
        tooLong.Length.Should().BeGreaterThan(254);

        var act = () => Email.Create(tooLong);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Equality_SameEmailDifferentCase_AreEqual()
    {
        var a = Email.Create("alice@example.com");
        var b = Email.Create("ALICE@EXAMPLE.COM");

        a.Should().Be(b);
    }
}

/// <summary>
/// Unit tests for the <see cref="FullName"/> value object.
/// </summary>
public sealed class FullNameValueObjectTests
{
    [Fact]
    public void Create_WithValidComponents_ReturnsFullName()
    {
        var name = FullName.Create("Alice", "Smith");

        name.FirstName.Should().Be("Alice");
        name.LastName.Should().Be("Smith");
        name.DisplayName.Should().Be("Alice Smith");
    }

    [Theory]
    [InlineData("", "Smith")]
    [InlineData("   ", "Smith")]
    [InlineData("Alice", "")]
    [InlineData("Alice", "   ")]
    public void Create_WithEmptyComponent_ThrowsValidationException(
        string firstName, string lastName)
    {
        var act = () => FullName.Create(firstName, lastName);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var longName = new string('A', 101);

        var act = () => FullName.Create(longName, "Smith");

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Equality_SameComponents_AreEqual()
    {
        var a = FullName.Create("Alice", "Smith");
        var b = FullName.Create("Alice", "Smith");

        a.Should().Be(b);
    }
}
