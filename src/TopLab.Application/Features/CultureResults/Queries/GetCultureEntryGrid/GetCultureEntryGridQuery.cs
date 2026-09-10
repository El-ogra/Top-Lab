using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureResults.Common;

namespace TopLab.Application.Features.CultureResults.Queries.GetCultureEntryGrid;

public sealed record GetCultureEntryGridQuery(int PatientTestId) : IRequest<Result<CultureEntryGridDto>>;
