using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PatientBilling.Commands.PrintInvoice;

/// <summary>
/// Allocates a new gapless invoice number, persists the issue record, and
/// prints the itemized invoice. Every call is a new numbered issue (SD-3).
/// Ungated (mirrors <c>RecordPayment</c> — registration-desk action).
/// </summary>
public sealed record PrintInvoiceCommand(int PatientId)
    : IRequest<Result<int>>;
