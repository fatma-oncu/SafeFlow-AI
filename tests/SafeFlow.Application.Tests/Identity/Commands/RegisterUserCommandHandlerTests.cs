using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SafeFlow.Application.Identity.Commands.RegisterUser;
using SafeFlow.Application.Identity.Interfaces;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;
using Xunit;

namespace SafeFlow.Application.Tests.Identity.Commands;

/// <summary>
/// Unit tests for <see cref="RegisterUserCommandHandler"/>.
/// </summary>
public sealed class RegisterUserCommandHandlerTests
{
    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly IIdentityService     _identityService     = Substitute.For<IIdentityService>();
    private readonly IRepository<User>    _userRepository      = Substitute.For<IRepository<User>>();
    private readonly IUnitOfWork          _unitOfWork          = Substitute.For<IUnitOfWork>();
    private readonly IAuditService        _auditService        = Substitute.For<IAuditService>();
    private readonly RegisterUserCommandHandler _sut;

    public RegisterUserCommandHandlerTests()
    {
        _sut = new RegisterUserCommandHandler(
            _identityService,
            _userRepository,
            _unitOfWork,
            _auditService,
            NullLogger<RegisterUserCommandHandler>.Instance);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static RegisterUserCommand ValidCommand(string email = "alice@example.com") =>
        new(
            Email:       email,
            Password:    "Password1!",
            FirstName:   "Alice",
            LastName:    "Smith",
            PhoneNumber: null,
            TenantId:    Guid.NewGuid(),
            IpAddress:   "127.0.0.1",
            UserAgent:   "TestAgent/1.0");

    // ── Success path ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenEmailIsUniqueAndCreationSucceeds_ReturnsSuccessWithUserId()
    {
        var expectedUserId = Guid.NewGuid();
        _identityService.IsEmailUniqueAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                        .Returns(true);
        _identityService.CreateUserAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string?>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedUserId));

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedUserId);
    }

    [Fact]
    public async Task Handle_WhenEmailIsUniqueAndCreationSucceeds_SavesUserToRepository()
    {
        var expectedUserId = Guid.NewGuid();
        _identityService.IsEmailUniqueAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                        .Returns(true);
        _identityService.CreateUserAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string?>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedUserId));

        await _sut.Handle(ValidCommand(), CancellationToken.None);

        await _userRepository.Received(1)
            .AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ── Email already taken ───────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenEmailIsAlreadyTaken_ReturnsFailure()
    {
        _identityService.IsEmailUniqueAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                        .Returns(false);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Register");
    }

    [Fact]
    public async Task Handle_WhenEmailIsAlreadyTaken_DoesNotCreateUser()
    {
        _identityService.IsEmailUniqueAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                        .Returns(false);

        await _sut.Handle(ValidCommand(), CancellationToken.None);

        await _identityService.DidNotReceive()
            .CreateUserAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string?>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    // ── Identity service creation fails ──────────────────────────────────────

    [Fact]
    public async Task Handle_WhenIdentityCreationFails_ReturnsFailure()
    {
        _identityService.IsEmailUniqueAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                        .Returns(true);
        _identityService.CreateUserAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string?>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Guid>(Error.Validation("Identity.Create", "Password policy violated.")));

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        await _userRepository.DidNotReceive()
            .AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
