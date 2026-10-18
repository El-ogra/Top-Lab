using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// PDF-first printer for the group-patient report slip batch (Phase 6, GAP-013).
/// The single string token is the JSON-serialized <c>GroupPatientSlipBatchDto</c>
/// produced by <c>PrintGroupPatientSlipsCommandHandler</c>
/// (<c>GroupPatientSlipPrintEnvelope</c>), mirroring
/// <c>ILabOrderPrintingService</c> / <c>IReceiptPrintingService</c>.
/// </summary>
public interface IGroupPatientSlipPrintingService
{
    Task<Result> PrintGroupPatientSlipsAsync(string batchToken, CancellationToken cancellationToken = default);
}
