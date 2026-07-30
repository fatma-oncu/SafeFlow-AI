using FluentAssertions;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.Domain.Identity.ValueObjects;
using SafeFlow.SharedKernel.Exceptions;
using Xunit;

namespace SafeFlow.Domain.Tests.Identity;

/// <summary>
/// Unit tests for the <see cref="Role"/> aggregate root.
/// </summary>
public sealed class RoleAggregateTests
{
    // ── Role.Create ───────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidArguments_ReturnsRole()
    {
        var id   = Guid.NewGuid();
        var role = Role.Create(id, "Administrator", "Full access", isSystemRole: true);

        role.Id.Should().Be(id);
        role.Name.Should().Be("Administrator");
        role.Description.Should().Be("Full access");
        role.IsSystemRole.Should().BeTrue();
        role.RolePermissions.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ThrowsValidationException(string name)
    {
        var act = () => Role.Create(Guid.NewGuid(), name);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var longName = new string('A', 101);

        var act = () => Role.Create(Guid.NewGuid(), longName);

        act.Should().Throw<ValidationException>();
    }

    // ── AddPermission ─────────────────────────────────────────────────────────

    [Fact]
    public void AddPermission_NewPermission_IsAddedToRolePermissions()
    {
        var role       = Role.Create(Guid.NewGuid(), "Viewer");
        var permission = Permission.Create("Documents", "Read");

        role.AddPermission(permission);

        role.RolePermissions.Should().ContainSingle();
        role.RolePermissions.Single().Permission.Should().Be(permission);
    }

    [Fact]
    public void AddPermission_DuplicatePermission_IsIdempotent()
    {
        var role       = Role.Create(Guid.NewGuid(), "Viewer");
        var permission = Permission.Create("Documents", "Read");

        role.AddPermission(permission);
        role.AddPermission(permission); // second call must be no-op

        role.RolePermissions.Should().ContainSingle();
    }

    [Fact]
    public void AddPermission_NullPermission_ThrowsArgumentNullException()
    {
        var role = Role.Create(Guid.NewGuid(), "Viewer");

        var act = () => role.AddPermission(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── RemovePermission ──────────────────────────────────────────────────────

    [Fact]
    public void RemovePermission_ExistingPermission_RemovesIt()
    {
        var role       = Role.Create(Guid.NewGuid(), "Editor");
        var permission = Permission.Create("Documents", "Write");
        role.AddPermission(permission);

        role.RemovePermission(permission);

        role.RolePermissions.Should().BeEmpty();
    }

    [Fact]
    public void RemovePermission_NonExistingPermission_IsIdempotent()
    {
        var role       = Role.Create(Guid.NewGuid(), "Editor");
        var permission = Permission.Create("Documents", "Write");

        var act = () => role.RemovePermission(permission);

        act.Should().NotThrow();
        role.RolePermissions.Should().BeEmpty();
    }

    // ── Rename ────────────────────────────────────────────────────────────────

    [Fact]
    public void Rename_WithValidName_UpdatesName()
    {
        var role = Role.Create(Guid.NewGuid(), "Old Name");

        role.Rename("New Name");

        role.Name.Should().Be("New Name");
    }

    [Fact]
    public void Rename_WithEmptyName_ThrowsValidationException()
    {
        var role = Role.Create(Guid.NewGuid(), "Viewer");

        var act = () => role.Rename("");

        act.Should().Throw<ValidationException>();
    }
}
