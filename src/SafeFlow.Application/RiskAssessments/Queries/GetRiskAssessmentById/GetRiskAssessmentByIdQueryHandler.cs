using MediatR;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Application.RiskAssessments.Specifications;
using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Queries.GetRiskAssessmentById;

public sealed class GetRiskAssessmentByIdQueryHandler(
    IReadRepository<RiskAssessment> assessmentRepository)
    : IRequestHandler<GetRiskAssessmentByIdQuery, Result<RiskAssessmentDto>>
{
    public async Task<Result<RiskAssessmentDto>> Handle(
        GetRiskAssessmentByIdQuery query,
        CancellationToken cancellationToken)
    {
        var spec = new RiskAssessmentByIdSpecification(query.Id);
        var assessment = await assessmentRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (assessment is null)
        {
            return Result.Failure<RiskAssessmentDto>(RiskAssessmentErrors.NotFound);
        }

        return Result.Success(RiskAssessmentDto.FromAggregate(assessment));
    }
}
