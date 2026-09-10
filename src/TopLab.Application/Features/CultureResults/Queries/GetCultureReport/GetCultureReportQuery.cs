using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureResults.Common;

namespace TopLab.Application.Features.CultureResults.Queries.GetCultureReport;

public sealed record GetCultureReportQuery(int PatientTestId) : IRequest<Result<CultureReportDto>>;
