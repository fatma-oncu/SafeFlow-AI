using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.Domain.RiskAssessments.ValueObjects;
using Xunit;

namespace SafeFlow.Domain.Tests.RiskAssessments;

public class RiskMatrixScoreTests
{
    [Theory]
    [InlineData(Likelihood.Rare, Severity.Negligible, 1, RiskLevel.Low)]
    [InlineData(Likelihood.Unlikely, Severity.Minor, 4, RiskLevel.Low)]
    [InlineData(Likelihood.Possible, Severity.Moderate, 9, RiskLevel.Medium)]
    [InlineData(Likelihood.Likely, Severity.Major, 16, RiskLevel.Critical)]
    [InlineData(Likelihood.AlmostCertain, Severity.Critical, 25, RiskLevel.Critical)]
    [InlineData(Likelihood.Likely, Severity.Moderate, 12, RiskLevel.High)]
    public void Calculate_ShouldReturnCorrectScoreAndRiskLevel(
        Likelihood likelihood,
        Severity severity,
        int expectedScore,
        RiskLevel expectedLevel)
    {
        // Act
        var result = RiskMatrixScore.Calculate(likelihood, severity);

        // Assert
        Assert.Equal(likelihood, result.Likelihood);
        Assert.Equal(severity, result.Severity);
        Assert.Equal(expectedScore, result.Score);
        Assert.Equal(expectedLevel, result.RiskLevel);
    }
}
