using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PatientBilling.Commands.PrintReceipt;

/// <summary>
/// Prints the cashier receipt for a patient visit. Ungated (mirrors
/// <c>RecordPayment</c> — cashier action at the same desk).
/// </summary>
public sealed record PrintReceiptCommand(int PatientId)
    : IRequest<Result>;
