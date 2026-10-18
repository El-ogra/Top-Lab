using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.ResultDelivery.Queries.GetGroupPatientSlips;

/// <summary>
/// Builds one group-patient report slip per selected patient (Phase 6, GAP-013).
/// Returns only the intended patients: ids are de-duplicated, ordered by the
/// patient registration date then id (the delivery-list ordering), and
/// soft-deleted/unknown ids are excluded rather than failing the whole batch
/// (BulkPrintPreflightQuery precedent — a stale tick must not abort the run).
/// Identity + identifier reuse <see cref="BarcodePayload.For"/> (the single
/// identifier rule for the scannable surfaces) and test lines reuse
/// <c>PatientBillingReader.ReadChargedTests</c> — the same line source the
/// Phase 1 lab-order slip uses, so no patient-data rendering is duplicated.
/// </summary>
public sealed class GetGroupPatientSlipsQueryHandler
    : IRequestHandler<GetGroupPatientSlipsQuery, Result<GroupPatientSlipBatchDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ISender _sender;

    public GetGroupPatientSlipsQueryHandler(IApplicationDbContext db, ISender sender)
    {
        _db = db;
        _sender = sender;
    }

    public async Task<Result<GroupPatientSlipBatchDto>> Handle(
        GetGroupPatientSlipsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _sender.Send(new GetSystemSettingsQuery(), cancellationToken);
        if (!settings.IsSuccess)
        {
            return Result<GroupPatientSlipBatchDto>.Failure(settings.Error!);
        }

        var printLabIdInsteadOfPatientId = settings.Value!.PrintLabIdInsteadOfPatientId;

        var distinctIds = request.PatientIds.Distinct().ToList();
        var patients = _db.Set<Patient>()
            .Where(p => distinctIds.Contains(p.Id.Value) && !p.IsDeleted)
            .OrderBy(p => p.RegistrationDateUtc)
            .ThenBy(p => p.Id.Value)
            .ToList();

        var slips = patients.Select(patient => new GroupPatientSlipDto(
            patient.Id.Value,
            patient.FullName,
            patient.LabId?.Value,
            BarcodePayload.For(
                patient.Id.Value,
                patient.LabId?.Value,
                printLabIdInsteadOfPatientId),
            patient.RegistrationDateUtc,
            PatientBillingReader.ReadChargedTests(_db, patient.Id.Value)
                .Select(t => new GroupPatientSlipLineDto(t.TestCode, t.TestName))
                .ToList())).ToList();

        return Result<GroupPatientSlipBatchDto>.Success(new GroupPatientSlipBatchDto(slips));
    }
}
