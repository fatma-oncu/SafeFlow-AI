using FluentAssertions;
using SafeFlow.Domain.Identity.ValueObjects;
using SafeFlow.SharedKernel.Exceptions;
using Xunit;

namespace SafeFlow.Domain.Tests.Identity;

/// <summary>
/// Unit tests for the <see cref="Permission"/> value object.
/// </summary>
public sealed class PermissionValueObjectTests
{
    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidComponents_ReturnsPermission()
    {
        var permission = Permission.Create("Users", "Read");

        permission.Module.Should().Be("Users");
        permission.Action.Should().Be("Read");
        permission.CanonicalName.Should().Be("Users.Read");
    }

    [Theory]
    [InlineData("", "Read")]
    [InlineData("   ", "Read")]
    [InlineData("Users", "")]
    [InlineData("Users", "   ")]
    public void Create_WithEmptyComponent_ThrowsValidationException(
        string module, string action)
    {
        var act = () => Permission.Create(module, action);

        act.Should().Throw<ValidationException>();
    }

    // ── Equality (case-insensitive) ───────────────────────────────────────────

    [Fact]
    public void Equality_SameComponentsDifferentCase_AreEqual()
    {
        var a = Permission.Create("Users", "Read");
        var b = Permission.Create("users", "read");

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentModule_AreNotEqual()
    {
        var a = Permission.Create("Users", "Read");
        var b = Permission.Create("Roles", "Read");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_DifferentAction_AreNotEqual()
    {
        var a = Permission.Create("Users", "Read");
        var b = Permission.Create("Users", "Write");

        a.Should().NotBe(b);
    }

    // ── CanonicalName ─────────────────────────────────────────────────────────

    [Fact]
    public void CanonicalName_ReturnsDotSeparatedModuleAndAction()
    {
        var permission = Permission.Create("Incidents", "Delete");

        permission.CanonicalName.Should().Be("Incidents.Delete");
    }
}
