using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SampleCollection.Common;

namespace TopLab.Application.Features.SampleCollection.Queries.GetPatientTestsForDraw;

public sealed record GetPatientTestsForDrawQuery(
    int PatientId) : IRequest<Result<SampleDrawBoardDto>>;
