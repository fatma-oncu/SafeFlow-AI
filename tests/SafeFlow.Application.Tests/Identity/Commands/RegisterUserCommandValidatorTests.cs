using FluentAssertions;
using SafeFlow.Application.Identity.Commands.RegisterUser;
using Xunit;

namespace SafeFlow.Application.Tests.Identity.Commands;

/// <summary>
/// Unit tests for <see cref="RegisterUserCommandValidator"/>.
/// Uses FluentValidation's built-in <c>Validate</c> method; no external
/// TestHelper package is required.
/// </summary>
public sealed class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    private static RegisterUserCommand ValidCommand() =>
        new(
            Email:       "alice@example.com",
            Password:    "Password1!",
            FirstName:   "Alice",
            LastName:    "Smith",
            PhoneNumber: null,
            TenantId:    Guid.NewGuid(),
            IpAddress:   "127.0.0.1",
            UserAgent:   null);

    // ── Valid command ─────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // ── Email ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    public void Validate_WithInvalidEmail_HasEmailError(string email)
    {
        var result = _validator.Validate(ValidCommand() with { Email = email });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    // ── Password ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("short")]          // too short
    [InlineData("alllowercase1!")] // no uppercase
    [InlineData("ALLUPPERCASE1!")] // no lowercase
    [InlineData("NoDigitsHere!")]  // no digit
    [InlineData("NoSpecial1234")]  // no special char
    public void Validate_WithWeakPassword_HasPasswordError(string password)
    {
        var result = _validator.Validate(ValidCommand() with { Password = password });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }

    // ── Names ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithEmptyFirstName_HasFirstNameError()
    {
        var result = _validator.Validate(ValidCommand() with { FirstName = "" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.FirstName));
    }

    [Fact]
    public void Validate_WithEmptyLastName_HasLastNameError()
    {
        var result = _validator.Validate(ValidCommand() with { LastName = "" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.LastName));
    }

    // ── TenantId ──────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithEmptyTenantId_HasTenantIdError()
    {
        var result = _validator.Validate(ValidCommand() with { TenantId = Guid.Empty });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.TenantId));
    }

    // ── IpAddress ─────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithEmptyIpAddress_HasIpAddressError()
    {
        var result = _validator.Validate(ValidCommand() with { IpAddress = "" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.IpAddress));
    }
}
