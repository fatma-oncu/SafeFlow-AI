using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SafeFlow.Application.Identity.Commands.Login;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.Application.Identity.Interfaces;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.Domain.Identity.Entities;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;
using SafeFlow.SharedKernel.Specifications;
using SafeFlow.SharedKernel.ValueObjects;
using Xunit;

namespace SafeFlow.Application.Tests.Identity.Commands;

/// <summary>
/// Unit tests for <see cref="LoginCommandHandler"/>.
/// </summary>
public sealed class LoginCommandHandlerTests
{
    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly IReadRepository<User>          _userRepository     = Substitute.For<IReadRepository<User>>();
    private readonly IReadRepository<Role>          _roleRepository     = Substitute.For<IReadRepository<Role>>();
    private readonly IRepository<RefreshToken>      _tokenRepository    = Substitute.For<IRepository<RefreshToken>>();
    private readonly IIdentityService               _identityService    = Substitute.For<IIdentityService>();
    private readonly IJwtTokenService               _jwtTokenService    = Substitute.For<IJwtTokenService>();
    private readonly IAuditService                  _auditService       = Substitute.For<IAuditService>();
    private readonly IUnitOfWork                    _unitOfWork         = Substitute.For<IUnitOfWork>();
    private readonly LoginCommandHandler            _sut;

    public LoginCommandHandlerTests()
    {
        _sut = new LoginCommandHandler(
            _userRepository,
            _roleRepository,
            _tokenRepository,
            _identityService,
            _jwtTokenService,
            _auditService,
            _unitOfWork,
            NullLogger<LoginCommandHandler>.Instance);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static LoginCommand ValidCommand() =>
        new("alice@example.com", "Password1!", "127.0.0.1", "TestAgent/1.0");

    private static User ActiveUser()
    {
        var user = User.Create(
            Guid.NewGuid(),
            Email.Create("alice@example.com"),
            FullName.Create("Alice", "Smith"));
        user.ClearDomainEvents();
        return user;
    }

    // ── User not found ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsInvalidCredentials()
    {
        _userRepository.FirstOrDefaultAsync(
                Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    // ── Account locked ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenAccountIsLocked_ReturnsAccountLockedError()
    {
        var lockedUser = ActiveUser();
        lockedUser.Lock("Security policy");

        _userRepository.FirstOrDefaultAsync(
                Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(lockedUser);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Locked");
    }

    // ── Account inactive ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenAccountIsInactive_ReturnsAccountInactiveError()
    {
        var inactiveUser = ActiveUser();
        inactiveUser.Deactivate();

        _userRepository.FirstOrDefaultAsync(
                Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(inactiveUser);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Inactive");
    }

    // ── Invalid password ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_ReturnsInvalidCredentials()
    {
        _userRepository.FirstOrDefaultAsync(
                Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(ActiveUser());

        _identityService.ValidateCredentialsAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(false));

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    // ── Success path ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ReturnsTokenResponse()
    {
        _userRepository.FirstOrDefaultAsync(
                Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(ActiveUser());

        _identityService.ValidateCredentialsAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(true));

        _jwtTokenService.GenerateAccessToken(
                Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<Guid>(), Arg.Any<IEnumerable<string>>(), Arg.Any<IEnumerable<string>>())
            .Returns("test-access-token");

        _jwtTokenService.GenerateRefreshToken()
            .Returns("test-refresh-token");
        _jwtTokenService.HashToken(Arg.Any<string>())
            .Returns("hashed-refresh-token");
        _jwtTokenService.AccessTokenExpirationMinutes.Returns(60);
        _jwtTokenService.RefreshTokenExpirationDays.Returns(7);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("test-access-token");
        result.Value.RefreshToken.Should().Be("test-refresh-token");
        result.Value.User.Should().NotBeNull();
    }
}
