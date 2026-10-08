namespace TopLab.Application.Features.PatientEnvelope.Common;

/// <summary>
/// Live laboratory-order (requisition) slip content for one patient visit
/// (Phase 1, REF-068). The test lines are the visit's ordered tests as read by
/// the shared billing reader; computed at print time — no stored state.
/// </summary>
public sealed record LabOrderLineDto(string TestCode, string TestName);

public sealed record LabOrderDto(
    int PatientId,
    string FullName,
    string? LabId,
    string Identifier,
    DateTime DateUtc,
    IReadOnlyList<LabOrderLineDto> Lines);
