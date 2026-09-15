namespace TopLab.Application.Features.SentOutSamples.Common;

public sealed record SentOutSampleDto(
    int Id,
    int PatientTestId,
    string PatientName,
    string TestName,
    int ExternalLabEntityId,
    string ExternalLabName,
    decimal CostPrice,
    decimal PatientPrice,
    DateTime SentAtUtc,
    decimal TotalPaid,
    decimal Remaining,
    bool IsFullySettled);

public sealed record SentOutLabAccountDto(
    int ExternalLabEntityId,
    string ExternalLabName,
    int SentCount,
    decimal TotalCost,
    decimal TotalPaid,
    decimal Remaining);
