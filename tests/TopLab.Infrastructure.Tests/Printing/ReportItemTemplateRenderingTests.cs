using System.Text.Json;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// GAP-014 (Phase 7): the report-item templates drive labels, visibility,
/// reference expressions, free-text overrides and list membership, while an
/// empty template set reproduces the pre-Phase-7 output exactly.
/// </summary>
public class ReportItemTemplateRenderingTests
{
    private static CombinedReportLineDto Line(
        string? frozenRange = "4 - 6",
        int? flag = 0,
        string? lowComment = null,
        string? highComment = null,
        IReadOnlyList<TopLab.Application.Features.ReportProduction.Common.ProfileReportLineDto>? profileLines = null)
        => new(
            PatientTestId: 101,
            TestId: 2,
            TestName: "سكر الدم",
            TestCode: "GLU",
            ResultKind: 0,
            ResultValue: "5.5",
            ResultFlag: flag,
            FrozenRangeText: frozenRange,
            ProfileLines: profileLines ?? new List<TopLab.Application.Features.ReportProduction.Common.ProfileReportLineDto>(),
            Culture: null,
            LowComment: lowComment,
            HighComment: highComment);

    private static ReportPrintEnvelope CombinedEnvelope(params CombinedReportLineDto[] lines)
    {
        var dto = new CombinedReportDto(7, "علي", "LAB-1", lines);
        return new ReportPrintEnvelope(ReportPrintEnvelope.Combined, JsonSerializer.Serialize(dto));
    }

    private static string Render(
        ReportPrintEnvelope envelope,
        IReadOnlyCollection<ReportItemTemplate>? templates = null,
        string? reportList = null,
        string? marker = null)
    {
        var settings = ReportSettings.CreateDefault();
        if (marker is not null)
        {
            settings.SetNormalRangeMarker(marker);
        }

        var content = ReportContentBuilder.FromEnvelope(
            envelope, settings, SystemSettings.CreateDefault(),
            itemTemplates: templates, reportList: reportList);
        return string.Join("\n", content.BuildDisplayLines());
    }

    [Fact]
    public void NoTemplates_ReproducesDefaultOutput()
    {
        var all = Render(CombinedEnvelope(Line()));

        Assert.Contains("التحليل: سكر الدم", all, StringComparison.Ordinal);
        Assert.Contains("النتيجة: 5.5", all, StringComparison.Ordinal);
        Assert.Contains("المدى: 4 - 6", all, StringComparison.Ordinal);
    }

    [Fact]
    public void HiddenItem_IsOmitted()
    {
        var templates = new[]
        {
            ReportItemTemplate.Create(ReportItemKeys.TestName, "التحليل", showHide: false)
        };

        var all = Render(CombinedEnvelope(Line()), templates);

        Assert.DoesNotContain("التحليل: سكر الدم", all, StringComparison.Ordinal);
        Assert.Contains("النتيجة: 5.5", all, StringComparison.Ordinal);
    }

    [Fact]
    public void CustomLabel_IsUsed()
    {
        var templates = new[]
        {
            ReportItemTemplate.Create(ReportItemKeys.TestName, "فحص")
        };

        var all = Render(CombinedEnvelope(Line()), templates);

        Assert.Contains("فحص: سكر الدم", all, StringComparison.Ordinal);
        Assert.DoesNotContain("التحليل: سكر الدم", all, StringComparison.Ordinal);
    }

    [Fact]
    public void ReferenceExpression_OverridesMarker()
    {
        var templates = new[]
        {
            ReportItemTemplate.Create(ReportItemKeys.ReferenceRange, "المدى", referenceValueExpression: "RR")
        };

        var all = Render(CombinedEnvelope(Line()), templates, marker: "*");

        Assert.Contains("RR 4 - 6", all, StringComparison.Ordinal);
        Assert.DoesNotContain("* 4 - 6", all, StringComparison.Ordinal);
    }

    [Fact]
    public void FreeTextLowComment_ReplacesStoredComment()
    {
        var templates = new[]
        {
            ReportItemTemplate.Create(ReportItemKeys.LowComment, "قيمة منخفضة", freeTextOverride: true)
        };

        var line = Line(flag: (int)ResultFlag.Low, lowComment: "منخفض جداً");
        var all = Render(CombinedEnvelope(line), templates);

        Assert.Contains("قيمة منخفضة", all, StringComparison.Ordinal);
        Assert.DoesNotContain("منخفض جداً", all, StringComparison.Ordinal);
    }

    [Fact]
    public void ListMembership_Match_ShowsItem()
    {
        var templates = new[]
        {
            ReportItemTemplate.Create(ReportItemKeys.TestName, "التحليل", listMembership: "A")
        };

        var all = Render(CombinedEnvelope(Line()), templates, reportList: "A");

        Assert.Contains("التحليل: سكر الدم", all, StringComparison.Ordinal);
    }

    [Fact]
    public void ListMembership_Mismatch_HidesItem()
    {
        var templates = new[]
        {
            ReportItemTemplate.Create(ReportItemKeys.TestName, "التحليل", listMembership: "A")
        };

        var all = Render(CombinedEnvelope(Line()), templates, reportList: "B");

        Assert.DoesNotContain("التحليل: سكر الدم", all, StringComparison.Ordinal);
        Assert.Contains("النتيجة: 5.5", all, StringComparison.Ordinal);
    }

    [Fact]
    public void GroupSubtitle_CustomLabel_IsUsed()
    {
        var settings = ReportSettings.CreateDefault();
        settings.SetPrintOptions(printGroupSubTitle: true, suppressReprintMessage: false);

        var dto = new CombinedReportDto(7, "علي", "LAB-1", new List<CombinedReportLineDto>
        {
            Line() with { TestGroupName = "كيمياء" }
        });
        var envelope = new ReportPrintEnvelope(ReportPrintEnvelope.Combined, JsonSerializer.Serialize(dto));
        var templates = new[] { ReportItemTemplate.Create(ReportItemKeys.GroupSubtitle, "مجموعة") };

        var content = ReportContentBuilder.FromEnvelope(
            envelope, settings, SystemSettings.CreateDefault(), itemTemplates: templates);
        var all = string.Join("\n", content.BuildDisplayLines());

        Assert.Contains("مجموعة: كيمياء", all, StringComparison.Ordinal);
    }

    [Fact]
    public void HistoryGrid_HiddenColumnIsDropped()
    {
        var entry = new HistoryEntryDto(
            PatientTestId: 1, PatientId: 7, TestId: 2, TestName: "سكر الدم", TestCode: "GLU",
            ResultKind: 0, ResultValue: "5.5", ResultFlag: 0, IsReviewed: true,
            EnteredAtUtc: new DateTime(2026, 1, 2), ReviewedAtUtc: null);
        var dto = new PatientHistoryDto(7, "علي", "LAB-1", "Date", true, new List<HistoryEntryDto> { entry });
        var envelope = new ReportPrintEnvelope(ReportPrintEnvelope.History, JsonSerializer.Serialize(dto));
        var templates = new[] { ReportItemTemplate.Create(ReportItemKeys.HistoryComment, "التعليق", showHide: false) };

        var content = ReportContentBuilder.FromEnvelope(
            envelope, ReportSettings.CreateDefault(), SystemSettings.CreateDefault(), itemTemplates: templates);
        var all = string.Join("\n", content.BuildDisplayLines());

        Assert.Contains("التاريخ", all, StringComparison.Ordinal);
        Assert.Contains("التحليل", all, StringComparison.Ordinal);
        Assert.DoesNotContain("التعليق", all, StringComparison.Ordinal);
    }
}
