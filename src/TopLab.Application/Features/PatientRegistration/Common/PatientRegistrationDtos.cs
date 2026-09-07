using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.PatientRegistration.Common;

public sealed record PatientSummaryDto(
    int PatientId,
    string? LabId,
    string FullName,
    Sex Sex,
    int AgeValue,
    AgeUnit AgeUnit,
    string? NationalId,
    string? PrimaryPhone,
    DateTime RegistrationDateUtc,
    AccountType AccountType,
    bool IsVip,
    bool IsDeleted);

public sealed record PatientPhoneNumberDto(int PatientPhoneNumberId, string PhoneNumber, byte SortOrder);

public sealed record PatientMedicalConditionDto(int MedicalConditionTypeId, string Name);

public sealed record PatientDetailDto(
    int PatientId,
    string? LabId,
    string? Title,
    string FullName,
    Sex Sex,
    int AgeValue,
    AgeUnit AgeUnit,
    string? NationalId,
    string? Address,
    AccountType AccountType,
    bool IsVip,
    DateTime RegistrationDateUtc,
    DateTime? PickupDateUtc,
    bool IsFastingIndicated,
    int? FastingHours,
    bool RecentContrastImaging,
    string? Notes,
    int? TreatingDoctorId,
    string? TreatingDoctorName,
    int? ReferralEntityId,
    string? ReferralEntityName,
    IReadOnlyList<PatientPhoneNumberDto> PhoneNumbers,
    IReadOnlyList<PatientMedicalConditionDto> MedicalConditions,
    bool IsDeleted);

public sealed record PatientTitleDto(int PatientTitleId, string TitleText, bool IsDefault);

public sealed record MedicalConditionTypeDto(int MedicalConditionTypeId, string Name, MedicalConditionCategory Category);

public sealed record PatientTestSummaryDto(
    int PatientTestId,
    int TestId,
    string TestName,
    string TestCode,
    decimal PriceAtOrderTime,
    bool IsUrine,
    bool IsStool,
    bool IsBlood,
    bool IsSemen,
    bool IsCsf,
    bool IsTakenOutsideLab,
    bool IsSampleDrawn,
    DateTime? SampleDrawnAtUtc,
    DateTime CreatedAtUtc);

public sealed record VisitHistoryDto(
    int PatientId,
    string? LabId,
    DateTime RegistrationDateUtc,
    IReadOnlyList<PatientTestSummaryDto> Tests);

public sealed record LabIdAvailabilityDto(string LabId, bool IsAvailable);

public sealed record RegistrationCatalogDto(
    IReadOnlyList<TestSummaryDto> Tests,
    IReadOnlyList<TestGroupDto> TestGroups,
    IReadOnlyList<PatientTitleDto> PatientTitles,
    IReadOnlyList<MedicalConditionTypeDto> MedicalConditionTypes,
    AccountType DefaultAccountType,
    bool DisableAutoTitleInsertion,
    string ReferralPlaceholderMale,
    string ReferralPlaceholderFemale);