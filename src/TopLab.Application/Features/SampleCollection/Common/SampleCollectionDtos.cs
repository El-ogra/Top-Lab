namespace TopLab.Application.Features.SampleCollection.Common;

public sealed record PatientWithUndrawnTestsDto(
    int PatientId,
    string? LabId,
    string FullName,
    DateTime RegistrationDateUtc,
    int UndrawnCount);

public sealed record PatientTestDrawDto(
    int PatientTestId,
    int PatientId,
    int TestId,
    string TestName,
    bool IsUrine,
    bool IsStool,
    bool IsBlood,
    bool IsSemen,
    bool IsCsf,
    bool IsTakenOutsideLab,
    bool IsSampleDrawn,
    DateTime? SampleDrawnAtUtc);

public sealed record SampleDrawBoardDto(
    int PatientId,
    string? LabId,
    string FullName,
    IReadOnlyList<PatientTestDrawDto> Drawn,
    IReadOnlyList<PatientTestDrawDto> NotDrawn);
