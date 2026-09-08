using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;

namespace TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;

public sealed record GetPatientAccountQuery(
    int PatientId) : IRequest<Result<PatientAccountDto>>;
