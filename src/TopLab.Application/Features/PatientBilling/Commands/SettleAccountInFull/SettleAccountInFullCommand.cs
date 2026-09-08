using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PatientBilling.Commands.SettleAccountInFull;

public sealed record SettleAccountInFullCommand(
    int PatientId) : IRequest<Result<int>>;
