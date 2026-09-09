using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.ProfileResults.Common;

public sealed record FrozenProfileRangeDto(
    int AnalyteId,
    string AnalyteName,
    string? Sex,
    string AgeUnit,
    int AgeMin,
    int AgeMax,
    decimal MinValue,
    decimal MaxValue,
    string? LowComment,
    string? HighComment,
    DateTimeOffset CapturedAtUtc);

public sealed record ProfileEntryItemDto(
    int ProfileResultItemId,
    int AnalyteId,
    string AnalyteName,
    string? ResultValue,
    string? Unit,
    int? Flag,
    bool IsVerified,
    bool IsPrinted,
    FrozenProfileRangeDto? FrozenRange);

public sealed record ProfileEntryGridDto(
    int PatientTestId,
    int PatientId,
    string PatientFullName,
    string ProfileName,
    IReadOnlyList<ProfileEntryItemDto> Items);

public sealed record ProfileReportLineDto(
    int ProfileResultItemId,
    int AnalyteId,
    string AnalyteName,
    string? ResultValue,
    string? Unit,
    int? Flag,
    FrozenProfileRangeDto? FrozenRange);

public sealed record ProfileReportDto(
    int PatientTestId,
    int PatientId,
    string PatientFullName,
    string? LabId,
    string ProfileName,
    string? Comment,
    bool IsReviewed,
    bool IsPrinted,
    bool IsDelivered,
    IReadOnlyList<ProfileReportLineDto> Lines);

public sealed record ProfileAmendmentDto(
    int Id,
    int AmendedByUserId,
    DateTime AmendedAtUtc,
    string OldResultValue,
    string? OldUnit,
    int? OldFlag,
    string NewResultValue,
    string? NewUnit,
    int? NewFlag,
    string? Reason);

/// <summary>
/// Reuses the sole live-range source (Decision 1): resolves the analyte's current
/// band via the shared ADR-0035 matcher and freezes it into the per-item snapshot.
/// Returns null when the analyte has no matching band.
/// </summary>
internal static class ProfileResultReferenceRangeCapture
{
    public static ProfileResultItemReferenceRangeSnapshot? Capture(
        IApplicationDbContext db,
        ProfileResultItem item,
        Patient patient,
        IDateTimeProvider clock)
    {
        var range = db.Set<AnalyteReferenceRange>().FirstOrDefault(r => r.AnalyteId.Equals(item.AnalyteId));
        if (range is null)
        {
            return null;
        }

        var bands = db.Set<AnalyteReferenceRangeBand>()
            .Where(b => b.AnalyteReferenceRangeId.Equals(range.Id))
            .ToList();
        var band = ResultFlagComputer.SelectMatch(patient.Sex, patient.AgeUnit, patient.AgeValue, bands);
        if (band is null)
        {
            return null;
        }

        return ProfileResultItemReferenceRangeSnapshot.Create(
            item.Id,
            item.AnalyteId,
            band.Sex,
            band.AgeUnit,
            band.AgeMin,
            band.AgeMax,
            band.MinValue,
            band.MaxValue,
            band.LowComment,
            band.HighComment,
            clock.UtcNow);
    }

    public static Profile? FindProfile(IApplicationDbContext db, PatientTest pt)
    {
        return db.Set<Profile>().FirstOrDefault(p => p.TestId.Equals(pt.TestId));
    }

    public static IReadOnlyList<Analyte> ConfiguredAnalytes(IApplicationDbContext db, Profile profile)
    {
        var ids = db.Set<ProfileAnalyte>()
            .Where(l => l.ProfileId.Equals(profile.Id))
            .Select(l => l.AnalyteId.Value)
            .ToList();

        var byId = db.Set<Analyte>()
            .Where(a => ids.Contains(a.Id.Value))
            .ToDictionary(a => a.Id.Value);

        return ids
            .Where(byId.ContainsKey)
            .Select(id => byId[id])
            .OrderBy(a => a.Id.Value)
            .ToList();
    }
}