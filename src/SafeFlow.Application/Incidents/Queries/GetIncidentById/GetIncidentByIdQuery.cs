using MediatR;
using SafeFlow.Application.Incidents.DTOs;
using SafeFlow.Application.Incidents.Errors;
using SafeFlow.Application.Incidents.Specifications;
using SafeFlow.Domain.Incidents.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Incidents.Queries.GetIncidentById;

public sealed record GetIncidentByIdQuery(Guid Id) : IRequest<Result<IncidentDto>>;

public sealed class GetIncidentByIdQueryHandler(
    IReadRepository<Incident> incidentRepository)
    : IRequestHandler<GetIncidentByIdQuery, Result<IncidentDto>>
{
    public async Task<Result<IncidentDto>> Handle(GetIncidentByIdQuery query, CancellationToken cancellationToken)
    {
        var spec = new IncidentByIdSpecification(query.Id);
        var incident = await incidentRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (incident is null)
        {
            return Result.Failure<IncidentDto>(IncidentErrors.NotFound);
        }

        return Result.Success(IncidentDto.FromAggregate(incident));
    }
}
