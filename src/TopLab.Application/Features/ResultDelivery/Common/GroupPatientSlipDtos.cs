namespace TopLab.Application.Features.ResultDelivery.Common;

/// <summary>
/// Group-patient report slip content (Phase 6, GAP-013).
/// One <see cref="GroupPatientSlipDto"/> per selected patient; the slip is a
/// standalone, independently archivable follow-up slip — the per-patient
/// "job order" of the reference guide (RLS pp.52-57 / RL show p.6), generated
/// as one page per patient inside a single PDF.
/// The test lines are the patient's ordered tests as read by the shared
/// billing reader (<c>PatientBillingReader.ReadChargedTests</c>) — the same
/// line source the Phase 1 lab-order slip uses — computed at print time; no
/// stored state, no schema change.
/// </summary>
public sealed record GroupPatientSlipLineDto(string TestCode, string TestName);

public sealed record GroupPatientSlipDto(
    int PatientId,
    string FullName,
    string? LabId,
    string Identifier,
    DateTime RegistrationDateUtc,
    IReadOnlyList<GroupPatientSlipLineDto> Lines);

/// <summary>
/// The full print batch: one slip per selected patient, in the order the
/// handler resolved them. An empty batch is valid and renders the writer's
/// empty-notice page (worksheet empty-state idiom) rather than a malformed
/// zero-page document.
/// </summary>
public sealed record GroupPatientSlipBatchDto(IReadOnlyList<GroupPatientSlipDto> Slips);
