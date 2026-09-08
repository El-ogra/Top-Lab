using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SampleCollection.Common;

namespace TopLab.Application.Features.SampleCollection.Queries.GetPatientsWithUncollectedSamples;

public sealed record GetPatientsWithUncollectedSamplesQuery(
    DateOnly? Day,
    int Page = 1,
    int PageSize = 100) : IRequest<Result<IReadOnlyList<PatientWithUndrawnTestsDto>>>;
