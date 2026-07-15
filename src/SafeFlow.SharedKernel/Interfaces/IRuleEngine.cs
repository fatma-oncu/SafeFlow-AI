namespace SafeFlow.SharedKernel.Interfaces;

/// <summary>
/// Provides a domain-agnostic abstraction for evaluating named business rules
/// against an arbitrary context object.
/// </summary>
/// <remarks>
/// <para>
/// The Rule Engine pattern externalises complex, configurable, or frequently-changing
/// business rules from domain aggregates into a dedicated evaluation engine.  This is
/// especially useful for SafeFlow's safety compliance rules, which may be configured
/// per-tenant and must remain maintainable without code deployments.
/// </para>
/// <para>
/// Rules are identified by a stable <c>ruleId</c> string
/// (e.g., <c>"IncidentReport.RequireWitness"</c>, <c>"Training.MaximumGapDays"</c>).
/// The Infrastructure implementation resolves and executes the rule from a rule
/// repository (database, YAML file, external rules engine) without exposing that
/// mechanism to the Application layer.
/// </para>
/// <para>
/// The generic <c>TContext</c> parameter ensures that callers pass a
/// strongly-typed context object rather than an untyped <c>object</c>, enabling
/// compile-time safety at the call site while keeping this interface decoupled from
/// any specific domain type.
/// </para>
/// </remarks>
public interface IRuleEngine
{
    /// <summary>
    /// Evaluates the rule identified by <paramref name="ruleId"/> against the provided
    /// <paramref name="context"/> object and returns the evaluation outcome.
    /// </summary>
    /// <typeparam name="TContext">
    /// The type of the context object supplied to the rule.  Must be a non-nullable
    /// reference or value type.
    /// </typeparam>
    /// <param name="ruleId">
    /// The stable, unique identifier of the rule to evaluate
    /// (e.g., <c>"Training.MaximumGapDays"</c>).
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    /// <param name="context">
    /// The domain object or data transfer object that provides the rule's input data.
    /// Must not be <c>null</c>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A task whose result is <c>true</c> if the rule passes (the condition is
    /// satisfied); <c>false</c> if the rule fails.
    /// </returns>
    Task<bool> EvaluateRuleAsync<TContext>(
        string ruleId,
        TContext context,
        CancellationToken cancellationToken = default)
        where TContext : notnull;
}
