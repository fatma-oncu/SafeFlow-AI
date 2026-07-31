using MediatR;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Application.RiskAssessments.Specifications;
using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Queries.GetRiskAssessments;

public sealed class GetRiskAssessmentsQueryHandler(
    IReadRepository<RiskAssessment> assessmentRepository)
    : IRequestHandler<GetRiskAssessmentsQuery, Result<PagedResult<RiskAssessmentSearchResultDto>>>
{
    public async Task<Result<PagedResult<RiskAssessmentSearchResultDto>>> Handle(
        GetRiskAssessmentsQuery query,
        CancellationToken cancellationToken)
    {
        var pagedSpec = new RiskAssessmentsPagedSpecification(query.Page, query.PageSize, query.DepartmentId, query.Status);
        var countSpec = new RiskAssessmentSearchCountSpecification(departmentId: query.DepartmentId, status: query.Status);

        var items = await assessmentRepository.ListAsync(pagedSpec, cancellationToken);
        int totalCount = await assessmentRepository.CountAsync(countSpec, cancellationToken);

        var dtos = items.Select(RiskAssessmentSearchResultDto.FromAggregate).ToList();
        var pagedResult = PagedResult<RiskAssessmentSearchResultDto>.Create(dtos, totalCount, query.Page, query.PageSize);

        return Result.Success(pagedResult);
    }
}
