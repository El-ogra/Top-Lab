using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PatientBilling.Commands.RecordPayment;

public sealed record RecordPaymentCommand(
    int PatientId,
    decimal Amount,
    decimal? DiscountAmount = null) : IRequest<Result<int>>;
