using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PatientBilling.Commands.RecordExtraCharge;

public sealed record RecordExtraChargeCommand(
    int PatientId,
    decimal Amount) : IRequest<Result<int>>;
