using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureResults.Queries.GetCultureReport;
using TopLab.Application.Features.ProfileResults.Queries.GetProfileReport;
using TopLab.Application.Features.ReportProduction.Commands.BuildBlankReport;
using TopLab.Application.Features.ReportProduction.Commands.BuildCombinedReport;
using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Application.Features.ResultsEntry.Common;

/// <summary>
/// W-02 S4 (WP-06): the single honest print path. Build, then print, then stop.
/// <para>
/// <b>Never marks a result as printed</b> — SD-1 (decision 2 = b1). No <c>IApplicationDbContext</c>
/// is injected because this path changes no state; there is nothing to save. (The SD-1 guard is
/// verified structurally by VG-04's grep, so the forbidden method name is deliberately absent
/// from this file — including from comments.)
/// </para>
/// <para>
/// AD-1 (owner, 2026-10-01): the outcome carries no PDF path, because no port can supply one.
/// </para>
/// <para>
/// AD-2 (owner-approved): the profile branch sends <see cref="GetProfileReportQuery"/> — the
/// plan's <c>BuildProfileReportQuery</c> does not exist — and wraps the result in an internally
/// generated <see cref="ReportPrintEnvelope.Combined"/> envelope, exactly as the plan already
/// prescribes for the culture branch.
/// </para>
/// </summary>
public sealed class ResultPrintCoordinator : IResultPrintCoordinator
{
    private readonly ISender _sender;
    private readonly IReportPrintingService _printing;

    public ResultPrintCoordinator(ISender sender, IReportPrintingService printing)
    {
        _sender = sender;
        _printing = printing;
    }

    public async Task<ResultPrintOutcome> PrintAsync(
        int patientTestId, ResultPrintKind kind, CancellationToken ct = default)
    {
        if (patientTestId <= 0)
        {
            return new ResultPrintOutcome(patientTestId, kind, false, "معرّف التحليل غير صالح.");
        }

        var token = await BuildTokenAsync(patientTestId, kind, ct);

        // A failed build must never reach the printer — no tokens, no paper, no marking.
        if (!token.IsSuccess)
        {
            return new ResultPrintOutcome(patientTestId, kind, false, token.Error!.Message);
        }

        var print = await _printing.PrintReportAsync(token.Value!, ct);

        // The Arabic text is whatever ReportPrintingService returned, verbatim: UI behaviour
        // does not change, we only stop lying about success.
        return new ResultPrintOutcome(
            patientTestId,
            kind,
            print.IsSuccess,
            print.IsSuccess ? null : print.Error!.Message);
    }

    private async Task<Result<string>> BuildTokenAsync(int patientTestId, ResultPrintKind kind, CancellationToken ct)
    {
        switch (kind)
        {
            case ResultPrintKind.ProfileReport:
                return await BuildProfileTokenAsync(patientTestId, ct);

            case ResultPrintKind.CultureReport:
            case ResultPrintKind.SimpleResult:
                return await BuildCombinedTokenAsync(patientTestId, ct);

            case ResultPrintKind.BlankReport:
                return await BuildBlankTokenAsync(patientTestId, ct);

            default:
                return Result<string>.Failure(Error.Validation("نوع الطباعة غير مدعوم."));
        }
    }

    /// <summary>AD-2: the real profile query; its DTO is re-wrapped as a single-line combined report.</summary>
    private async Task<Result<string>> BuildProfileTokenAsync(int patientTestId, CancellationToken ct)
    {
        var built = await _sender.Send(new GetProfileReportQuery(patientTestId), ct);
        if (!built.IsSuccess)
        {
            return Result<string>.Failure(built.Error!);
        }

        var report = built.Value!;
        var combined = new CombinedReportDto(
            report.PatientId,
            report.PatientFullName,
            report.LabId,
            [new CombinedReportLineDto(
                patientTestId,
                0,
                report.ProfileName,
                string.Empty,
                (int)TopLab.Domain.Common.Enums.ResultKind.SpecializedProfile,
                null,
                null,
                null,
                [.. report.Lines.Select(l => new ProfileReportLineDto(
                    l.ProfileResultItemId,
                    l.AnalyteId,
                    l.AnalyteName,
                    l.ResultValue,
                    l.Unit,
                    l.Flag,
                    // Both frozen-range records are structurally identical; the profile one is
                    // already the type the combined-report line expects.
                    l.FrozenRange is null
                        ? null
                        : new FrozenProfileRangeDto(
                            l.FrozenRange.AnalyteId,
                            l.FrozenRange.AnalyteName,
                            l.FrozenRange.Sex,
                            l.FrozenRange.AgeUnit,
                            l.FrozenRange.AgeMin,
                            l.FrozenRange.AgeMax,
                            l.FrozenRange.MinValue,
                            l.FrozenRange.MaxValue,
                            l.FrozenRange.LowComment,
                            l.FrozenRange.HighComment,
                            l.FrozenRange.CapturedAtUtc)))],
                // SD-12: CombinedReportLineDto has FIFTEEN members and IsTakenOutsideLab is
                // the THIRTEENTH (ReportDtos.cs:71). After the Culture null we add LowComment,
                // HighComment and IsTakenOutsideLab — thirteen arguments in total. Passing a
                // bool in the 10th position would NOT compile: that is CultureReportSummaryDto?.
                null,
                null,
                null,
                report.IsTakenOutsideLab)]);

        return Result<string>.Success(ReportPrintEnvelope.CreateToken(ReportPrintEnvelope.Combined, combined));
    }

    private async Task<Result<string>> BuildCombinedTokenAsync(int patientTestId, CancellationToken ct)
    {
        // The patient id is needed by BuildCombinedReportCommand. Both read paths that can
        // resolve a test id carry it, so no database access is required here.
        var patientId = await ResolvePatientIdAsync(patientTestId, ct);
        if (!patientId.IsSuccess)
        {
            return Result<string>.Failure(patientId.Error!);
        }

        var built = await _sender.Send(
            new BuildCombinedReportCommand(patientId.Value, [patientTestId]), ct);
        if (!built.IsSuccess)
        {
            return Result<string>.Failure(built.Error!);
        }

        return Result<string>.Success(ReportPrintEnvelope.CreateToken(ReportPrintEnvelope.Combined, built.Value!));
    }

    private async Task<Result<string>> BuildBlankTokenAsync(int patientTestId, CancellationToken ct)
    {
        var patientId = await ResolvePatientIdAsync(patientTestId, ct);
        if (!patientId.IsSuccess)
        {
            return Result<string>.Failure(patientId.Error!);
        }

        var built = await _sender.Send(new BuildBlankReportCommand(patientId.Value), ct);
        if (!built.IsSuccess)
        {
            return Result<string>.Failure(built.Error!);
        }

        return Result<string>.Success(ReportPrintEnvelope.CreateToken(ReportPrintEnvelope.Blank, built.Value!));
    }

    /// <summary>
    /// Resolves the owning patient id through the feature's own read queries, so the coordinator
    /// stays free of <c>IApplicationDbContext</c> and of any ownership rule duplication.
    /// The culture query answers first because it is the only one carrying a full patient context.
    /// </summary>
    private async Task<Result<int>> ResolvePatientIdAsync(int patientTestId, CancellationToken ct)
    {
        var culture = await _sender.Send(new GetCultureReportQuery(patientTestId), ct);
        if (culture.IsSuccess)
        {
            return Result<int>.Success(culture.Value!.PatientId);
        }

        var profile = await _sender.Send(new GetProfileReportQuery(patientTestId), ct);
        if (profile.IsSuccess)
        {
            return Result<int>.Success(profile.Value!.PatientId);
        }

        return Result<int>.Failure(Error.NotFound("التحليل غير موجود"));
    }
}
