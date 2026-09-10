namespace TopLab.Application.Features.CultureResults.Common;

public sealed record CultureSensitivityRowDto(int? CultureAntibioticResultId, int AntibioticId,
    string AntibioticName, int? SensitivityCategory, bool IsPregnancyFlagged, bool IsChildrenFlagged);

public sealed record CultureEntryGridDto(int PatientTestId, int PatientId, string TestName,
    string TestCode, string? Sample, string? OrganismA, string? OrganismB, string? OrganismC,
    string? CultureCondition, string? ColonyCount, IReadOnlyList<CultureSensitivityRowDto> Rows,
    bool ParentIsReviewed, bool ParentIsPrinted);

public sealed record CultureReportDto(int PatientTestId, int PatientId, string PatientFullName,
    string? LabId, string PatientSex, int PatientAgeValue, string PatientAgeUnit,
    string TestName, string ReportName, DateTime RegistrationDateUtc,
    string? Sample, string? OrganismA, string? OrganismB, string? OrganismC,
    string? CultureCondition, string? ColonyCount, IReadOnlyList<CultureSensitivityRowDto> Rows,
    bool PrintLabIdInsteadOfPatientId, bool IsReviewed, bool IsPrinted, int PrintCount);
