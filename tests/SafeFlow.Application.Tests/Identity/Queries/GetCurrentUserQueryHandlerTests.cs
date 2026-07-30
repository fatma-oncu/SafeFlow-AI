using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.Application.Identity.Queries.GetCurrentUser;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Specifications;
using SafeFlow.SharedKernel.ValueObjects;
using Xunit;

namespace SafeFlow.Application.Tests.Identity.Queries;

/// <summary>
/// Unit tests for <see cref="GetCurrentUserQueryHandler"/>.
/// </summary>
public sealed class GetCurrentUserQueryHandlerTests
{
    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly ICurrentUserService    _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IReadRepository<User>  _userRepository     = Substitute.For<IReadRepository<User>>();
    private readonly GetCurrentUserQueryHandler _sut;

    public GetCurrentUserQueryHandlerTests()
    {
        _sut = new GetCurrentUserQueryHandler(
            _currentUserService,
            _userRepository,
            NullLogger<GetCurrentUserQueryHandler>.Instance);
    }

    private static User MakeUser(Guid userId)
    {
        var user = User.Create(
            userId,
            Email.Create("alice@example.com"),
            FullName.Create("Alice", "Smith"));
        user.ClearDomainEvents();
        return user;
    }

    // ── Not authenticated ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ReturnsNotAuthenticatedError()
    {
        _currentUserService.IsAuthenticated.Returns(false);
        _currentUserService.UserId.Returns((Guid?)null);

        var result = await _sut.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotAuthenticated");
    }

    // ── Authenticated but user missing from domain store ──────────────────────

    [Fact]
    public async Task Handle_WhenUserNotFoundInDomainStore_ReturnsNotFoundError()
    {
        var userId = Guid.NewGuid();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);

        _userRepository.FirstOrDefaultAsync(
                Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _sut.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    // ── Success path ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenAuthenticatedAndUserExists_ReturnsUserDto()
    {
        var userId = Guid.NewGuid();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(userId);

        var user = MakeUser(userId);
        _userRepository.FirstOrDefaultAsync(
                Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _sut.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<UserDto>();
        result.Value.Id.Should().Be(userId);
    }
}
