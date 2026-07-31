using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.Domain.Identity.Entities;
using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.Infrastructure.Identity;
using SafeFlow.Infrastructure.Persistence.Configurations;
using SafeFlow.SharedKernel.Entities;
using SafeFlow.SharedKernel.Interfaces;

namespace SafeFlow.Infrastructure.Persistence;

/// <summary>
/// The primary EF Core <see cref="DbContext"/> for the SafeFlow platform.
/// </summary>
/// <remarks>
/// <para>
/// Inherits from <see cref="IdentityDbContext{TUser,TRole,TKey}"/> so that the ASP.NET
/// Core Identity tables (<c>AspNetUsers</c>, <c>AspNetRoles</c>, etc.) share the same
/// SQL Server database and transaction scope as the domain tables.
/// </para>
/// <para>
/// All entity type configurations are applied via <see cref="IEntityTypeConfiguration{TEntity}"/>
/// implementations discovered through <see cref="ModelBuilder.ApplyConfigurationsFromAssembly"/>.
/// No Fluent API is written directly inside this class.
/// </para>
/// <para>
/// Audit stamp population (CreatedAt, LastModifiedAt, CreatedBy, LastModifiedBy)
/// happens in <see cref="SaveChangesAsync"/> before committing. Domain events are
/// dispatched after a successful commit via <see cref="IDomainEventDispatcher"/>.
/// </para>
/// </remarks>
public sealed class SafeFlowDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    /// <summary>
    /// Initialises a new <see cref="SafeFlowDbContext"/>.
    /// </summary>
    /// <param name="options">The EF Core context options.</param>
    /// <param name="currentUserService">Resolves the authenticated user for audit stamps.</param>
    /// <param name="domainEventDispatcher">Dispatches domain events after commit.</param>
    public SafeFlowDbContext(
        DbContextOptions<SafeFlowDbContext> options,
        ICurrentUserService currentUserService,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _currentUserService = currentUserService;
        _domainEventDispatcher = domainEventDispatcher;
    }

    // ── Domain DbSets ────────────────────────────────────────────────────────

    /// <summary>Gets the <see cref="User"/> aggregate root table.</summary>
    public DbSet<User> DomainUsers => Set<User>();

    /// <summary>Gets the <see cref="Role"/> aggregate root table.</summary>
    public DbSet<Role> DomainRoles => Set<Role>();

    /// <summary>Gets the <see cref="UserRole"/> join-entity table.</summary>
    public DbSet<UserRole> DomainUserRoles => Set<UserRole>();

    /// <summary>Gets the <see cref="RolePermission"/> join-entity table.</summary>
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    /// <summary>Gets the <see cref="RefreshToken"/> entity table.</summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>Gets the <see cref="Employee"/> aggregate root table.</summary>
    public DbSet<Employee> Employees => Set<Employee>();

    /// <summary>Gets the <see cref="RiskAssessment"/> aggregate root table.</summary>
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();

    // ── EF Core configuration ─────────────────────────────────────────────────

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Discover and apply all IEntityTypeConfiguration<T> in this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(SafeFlowDbContext).Assembly);
    }

    // ── SaveChanges with audit + domain-event dispatch ────────────────────────

    /// <inheritdoc/>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditFields();

        // Collect domain events before committing (they are cleared after dispatch)
        var aggregatesWithEvents = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        int result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch events AFTER successful commit
        foreach (var aggregate in aggregatesWithEvents)
        {
            var events = aggregate.DomainEvents.ToList();
            aggregate.ClearDomainEvents();

            await _domainEventDispatcher.DispatchAsync(events, cancellationToken);
        }

        return result;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void StampAuditFields()
    {
        string actor = _currentUserService.UserId?.ToString()
                       ?? _currentUserService.UserName
                       ?? "system";

        DateTime now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = actor;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = now;
                    entry.Entity.LastModifiedBy = actor;
                    break;
            }
        }
    }
}
