using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Domain.RiskAssessments.ValueObjects;

/// <summary>
/// Immutable Value Object encapsulating server-side Risk Score calculation and Risk Level derivation.
/// Prevents primitive obsession and client-side score manipulation.
/// </summary>
public sealed class RiskMatrixScore : ValueObject
{
    private RiskMatrixScore()
    {
        Likelihood = Likelihood.Rare;
        Severity = Severity.Negligible;
        Score = 1;
        RiskLevel = RiskLevel.Low;
    }

    private RiskMatrixScore(Likelihood likelihood, Severity severity)
    {
        Likelihood = likelihood;
        Severity = severity;
        Score = (int)likelihood * (int)severity;
        RiskLevel = CalculateRiskLevel(Score);
    }

    /// <summary>Gets the likelihood rating.</summary>
    public Likelihood Likelihood { get; private set; }

    /// <summary>Gets the severity rating.</summary>
    public Severity Severity { get; private set; }

    /// <summary>Gets the calculated numeric risk score (1 to 25).</summary>
    public int Score { get; private set; }

    /// <summary>Gets the derived risk level classification.</summary>
    public RiskLevel RiskLevel { get; private set; }

    /// <summary>
    /// Factory method to calculate a <see cref="RiskMatrixScore"/> from likelihood and severity ratings.
    /// </summary>
    public static RiskMatrixScore Calculate(Likelihood likelihood, Severity severity)
    {
        if (!Enum.IsDefined(typeof(Likelihood), likelihood))
            throw new ArgumentOutOfRangeException(nameof(likelihood), likelihood, "Invalid likelihood rating.");

        if (!Enum.IsDefined(typeof(Severity), severity))
            throw new ArgumentOutOfRangeException(nameof(severity), severity, "Invalid severity rating.");

        return new RiskMatrixScore(likelihood, severity);
    }

    private static RiskLevel CalculateRiskLevel(int score) => score switch
    {
        >= 1 and <= 4 => RiskLevel.Low,
        >= 5 and <= 9 => RiskLevel.Medium,
        >= 10 and <= 15 => RiskLevel.High,
        _ => RiskLevel.Critical
    };

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Likelihood;
        yield return Severity;
        yield return Score;
        yield return RiskLevel;
    }

    /// <inheritdoc/>
    public override string ToString() => $"Score: {Score} ({RiskLevel})";
}
