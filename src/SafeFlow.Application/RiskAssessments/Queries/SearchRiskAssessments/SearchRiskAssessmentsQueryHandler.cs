using MediatR;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Application.RiskAssessments.Specifications;
using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Queries.SearchRiskAssessments;

public sealed class SearchRiskAssessmentsQueryHandler(
    IReadRepository<RiskAssessment> assessmentRepository)
    : IRequestHandler<SearchRiskAssessmentsQuery, Result<PagedResult<RiskAssessmentSearchResultDto>>>
{
    public async Task<Result<PagedResult<RiskAssessmentSearchResultDto>>> Handle(
        SearchRiskAssessmentsQuery query,
        CancellationToken cancellationToken)
    {
        var searchSpec = new RiskAssessmentSearchSpecification(
            query.SearchTerm,
            query.DepartmentId,
            query.ResponsibleEmployeeId,
            query.CreatedByEmployeeId,
            query.AssessmentNumber,
            query.Status,
            query.RiskLevel,
            query.FromDate,
            query.ToDate,
            query.Page,
            query.PageSize);

        var countSpec = new RiskAssessmentSearchCountSpecification(
            query.SearchTerm,
            query.DepartmentId,
            query.ResponsibleEmployeeId,
            query.CreatedByEmployeeId,
            query.AssessmentNumber,
            query.Status,
            query.RiskLevel,
            query.FromDate,
            query.ToDate);

        var items = await assessmentRepository.ListAsync(searchSpec, cancellationToken);
        int totalCount = await assessmentRepository.CountAsync(countSpec, cancellationToken);

        var dtos = items.Select(RiskAssessmentSearchResultDto.FromAggregate).ToList();
        var pagedResult = PagedResult<RiskAssessmentSearchResultDto>.Create(dtos, totalCount, query.Page, query.PageSize);

        return Result.Success(pagedResult);
    }
}
