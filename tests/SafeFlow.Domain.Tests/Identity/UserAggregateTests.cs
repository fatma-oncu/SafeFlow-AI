using FluentAssertions;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.Domain.Identity.Events;
using SafeFlow.SharedKernel.Exceptions;
using SafeFlow.SharedKernel.ValueObjects;
using Xunit;

namespace SafeFlow.Domain.Tests.Identity;

/// <summary>
/// Unit tests for the <see cref="User"/> aggregate root.
/// </summary>
public sealed class UserAggregateTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static User CreateValidUser(Guid? id = null) =>
        User.Create(
            id ?? Guid.NewGuid(),
            Email.Create("john.doe@example.com"),
            FullName.Create("John", "Doe"));

    // ── User.Create ───────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidArguments_ReturnsActiveUser()
    {
        var id    = Guid.NewGuid();
        var email = Email.Create("alice@example.com");
        var name  = FullName.Create("Alice", "Smith");

        var user = User.Create(id, email, name);

        user.Id.Should().Be(id);
        user.Email.Value.Should().Be("alice@example.com");
        user.FullName.FirstName.Should().Be("Alice");
        user.FullName.LastName.Should().Be("Smith");
        user.IsActive.Should().BeTrue();
        user.IsLocked.Should().BeFalse();
        user.LastLoginAt.Should().BeNull();
        user.PhoneNumber.Should().BeNull();
    }

    [Fact]
    public void Create_RaisesUserRegisteredDomainEvent()
    {
        var user = CreateValidUser();

        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserRegisteredDomainEvent>();
    }

    [Fact]
    public void Create_WithNullEmail_ThrowsArgumentNullException()
    {
        var name = FullName.Create("Alice", "Smith");

        var act = () => User.Create(Guid.NewGuid(), null!, name);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── Lock / Unlock ─────────────────────────────────────────────────────────

    [Fact]
    public void Lock_ActiveUser_SetsIsLockedTrue()
    {
        var user = CreateValidUser();

        user.Lock("Suspicious activity");

        user.IsLocked.Should().BeTrue();
    }

    [Fact]
    public void Lock_RaisesUserLockedDomainEvent()
    {
        var user = CreateValidUser();
        user.ClearDomainEvents(); // remove the registration event

        user.Lock("Test reason");

        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserLockedDomainEvent>();
    }

    [Fact]
    public void Lock_AlreadyLockedUser_DoesNotRaiseDuplicateEvent()
    {
        var user = CreateValidUser();
        user.Lock("First lock");
        user.ClearDomainEvents();

        user.Lock("Second lock"); // idempotent

        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Unlock_LockedUser_SetsIsLockedFalse()
    {
        var user = CreateValidUser();
        user.Lock("reason");
        user.ClearDomainEvents();

        user.Unlock();

        user.IsLocked.Should().BeFalse();
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserUnlockedDomainEvent>();
    }

    // ── RecordLogin ───────────────────────────────────────────────────────────

    [Fact]
    public void RecordLogin_SetsLastLoginAtToUtcNow()
    {
        var user = CreateValidUser();
        var before = DateTime.UtcNow.AddSeconds(-1);

        user.RecordLogin();

        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt!.Value.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void RecordLogin_RaisesUserLoggedInDomainEvent()
    {
        var user = CreateValidUser();
        user.ClearDomainEvents();

        user.RecordLogin();

        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserLoggedInDomainEvent>();
    }

    // ── AssignRole / RemoveRole ───────────────────────────────────────────────

    [Fact]
    public void AssignRole_NewRole_AddsToUserRoles()
    {
        var user   = CreateValidUser();
        var roleId = Guid.NewGuid();

        user.AssignRole(roleId);

        user.UserRoles.Should().ContainSingle(ur => ur.RoleId == roleId);
    }

    [Fact]
    public void AssignRole_DuplicateRole_IsIdempotent()
    {
        var user   = CreateValidUser();
        var roleId = Guid.NewGuid();

        user.AssignRole(roleId);
        user.AssignRole(roleId); // second call should be no-op

        user.UserRoles.Should().ContainSingle(ur => ur.RoleId == roleId);
    }

    [Fact]
    public void RemoveRole_ExistingRole_RemovesFromUserRoles()
    {
        var user   = CreateValidUser();
        var roleId = Guid.NewGuid();
        user.AssignRole(roleId);

        user.RemoveRole(roleId);

        user.UserRoles.Should().BeEmpty();
    }
}
