namespace TopLab.Application.Features.Statistics.Common;

/// <summary>
/// One result of the chosen test whose numeric value lies inside the inclusive band.
/// The date column is the result-entry instant (<c>PatientTest.EnteredAtUtc</c>), BR-F05-3.
/// </summary>
public sealed record BandedResultRowDto(
    int PatientTestId,
    DateTime EnteredAtUtc,
    int PatientId,
    string PatientFullName,
    string PatientSex,
    int PatientAgeValue,
    string PatientAgeUnit,
    string ReferralEntityName,
    string TestName,
    string ResultValue,
    string StatusText);

/// <summary>
/// Banded result monitor response. <c>Rows</c> is the grid and <c>TotalCount</c> its size;
/// an empty set is a success, never an error (H-4).
/// </summary>
public sealed record BandedResultMonitorDto(
    DateOnly From,
    DateOnly To,
    int TestId,
    string TestName,
    decimal MinValue,
    decimal MaxValue,
    IReadOnlyList<BandedResultRowDto> Rows,
    int TotalCount);