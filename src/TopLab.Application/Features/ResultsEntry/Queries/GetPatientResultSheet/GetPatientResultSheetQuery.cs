using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;

namespace TopLab.Application.Features.ResultsEntry.Queries.GetPatientResultSheet;

public sealed record GetPatientResultSheetQuery(int PatientId) : IRequest<Result<PatientResultSheetDto>>;
