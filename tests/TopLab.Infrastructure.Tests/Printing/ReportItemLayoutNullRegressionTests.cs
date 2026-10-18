using System.Text.Json;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Printing;
using Xunit;
using FrozenProfileRangeDto = TopLab.Application.Features.ReportProduction.Common.FrozenProfileRangeDto;
using ProfileFrozenRangeDto = TopLab.Application.Features.ProfileResults.Common.FrozenProfileRangeDto;
using ProfileReportLineDto = TopLab.Application.Features.ReportProduction.Common.ProfileReportLineDto;
using ProfileReportDto = TopLab.Application.Features.ProfileResults.Common.ProfileReportDto;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// GAP-015 (Phase 8) regression: with LineSpacing, LeftOffsetCm and MarkerSymbol
/// all null, every one of the five report shapes must render exactly the content
/// it rendered before Phase 8 — same text, same lines, no per-item layout.
/// </summary>
public class ReportItemLayoutNullRegressionTests : IDisposable
{
    static ReportItemLayoutNullRegressionTests()
    {
        // Mirrors what every production writer does in its static constructor;
        // the tests build ReportDocument directly instead of going through a writer.
        // Mirrors what every production writer does in its static constructor;
        // the tests build ReportDocument directly instead of going through a writer.
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        QuestPDF.Settings.UseSystemFonts = true;
    }

    private readonly string _dir;

    public ReportItemLayoutNullRegressionTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "toplab-item-layout-null-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    /// <summary>Every template row with the three Phase 8 fields left null.</summary>
    private static IReadOnlyCollection<ReportItemTemplate> NullLayoutTemplates()
        => ReportItemKeys.All
            .Select(key =>
            {
                var seed = ReportItemTemplateCatalog.GetDefault(key);
                return ReportItemTemplate.Create(key, seed.Label);
            })
            .ToList();

    private static readonly FrozenRangeDto FrozenRange =
        new(10, null, "Year", 0, 100, 4m, 10m, "منخفض", "مرتفع", DateTimeOffset.UtcNow);

    private static readonly FrozenProfileRangeDto FrozenProfileRange =
        new(11, "ص.GL", null, "Year", 0, 100, 1m, 3m, null, null, DateTimeOffset.UtcNow);

    private static readonly ProfileFrozenRangeDto ProfileFrozenRange =
        new(11, "ص.GL", null, "Year", 0, 100, 1m, 3m, null, null, DateTimeOffset.UtcNow);

    private static ReportDocumentContent Combined(bool withTemplates)
    {
        var dto = new CombinedReportDto(7, "علي", "LAB-1", new List<CombinedReportLineDto>
        {
            new(101, 2, "سكر الدم", "GLU", 0, "5.5", (int)ResultFlag.Low, "4 - 10",
                new List<ProfileReportLineDto>
                {
                    new(1, 11, "ص.Na", "140", "mmol/L", (int)ResultFlag.Normal, FrozenProfileRange)
                },
                null, LowComment: "انخفاض طفيف", TestGroupName: "كيمياء", IsTakenOutsideLab: true,
                TestComments: new List<string> { "ملاحظة" })
        });

        return ReportContentBuilder.FromEnvelope(
            new ReportPrintEnvelope(ReportPrintEnvelope.Combined, JsonSerializer.Serialize(dto)),
            ReportSettings.CreateDefault(),
            SystemSettings.CreateDefault(),
            itemTemplates: withTemplates ? NullLayoutTemplates() : null);
    }

    private static ReportDocumentContent Blank(bool withTemplates)
    {
        var dto = new BlankReportDto(7, "علي", "LAB-1", "ذكر", 30, "سنة", "د. محمد", "مستشفى");
        return ReportContentBuilder.FromEnvelope(
            new ReportPrintEnvelope(ReportPrintEnvelope.Blank, JsonSerializer.Serialize(dto)),
            ReportSettings.CreateDefault(),
            SystemSettings.CreateDefault(),
            itemTemplates: withTemplates ? NullLayoutTemplates() : null);
    }

    private static ReportDocumentContent History(bool withTemplates)
    {
        var entry = new HistoryEntryDto(
            PatientTestId: 101, PatientId: 7, TestId: 2, TestName: "سكر الدم", TestCode: "GLU",
            ResultKind: 0, ResultValue: "5.5", ResultFlag: (int)ResultFlag.High, IsReviewed: true,
            EnteredAtUtc: new DateTime(2026, 1, 2), ReviewedAtUtc: null, HighComment: "مرتفع");
        var dto = new PatientHistoryDto(7, "علي", "LAB-1", "ByLabCode", true, new List<HistoryEntryDto> { entry });

        return ReportContentBuilder.FromEnvelope(
            new ReportPrintEnvelope(ReportPrintEnvelope.History, JsonSerializer.Serialize(dto)),
            ReportSettings.CreateDefault(),
            SystemSettings.CreateDefault(),
            itemTemplates: withTemplates ? NullLayoutTemplates() : null);
    }

    private static ReportDocumentContent PatientExport(bool withTemplates)
    {
        var data = new PatientReportPdfData(7, "علي", "LAB-1", new List<PatientReportPdfLine>
        {
            new(101, "سكر الدم", "GLU", "5.5", (int)ResultFlag.Low, null, FrozenRange,
                new List<string> { "ص.Na" }, "المزرعة: موجبة")
        });

        return ReportContentBuilder.FromPatientExport(
            data,
            ReportSettings.CreateDefault(),
            SystemSettings.CreateDefault(),
            itemTemplates: withTemplates ? NullLayoutTemplates() : null);
    }

    private static ReportDocumentContent ProfileReport(bool withTemplates)
    {
        var dto = new ProfileReportDto(
            PatientTestId: 55, PatientId: 7, PatientFullName: "علي", LabId: "LAB-1",
            ProfileName: "صورة الدم", Comment: "تعليق عام", IsReviewed: true, IsPrinted: false,
            IsDelivered: false,
            Lines: new List<TopLab.Application.Features.ProfileResults.Common.ProfileReportLineDto>
            {
                new(1, 11, "ص.Na", "140", "mmol/L", (int)ResultFlag.Normal, ProfileFrozenRange)
            });

        return ReportContentBuilder.FromProfileReport(
            dto,
            ReportSettings.CreateDefault(),
            SystemSettings.CreateDefault(),
            itemTemplates: withTemplates ? NullLayoutTemplates() : null);
    }

    public static TheoryData<string> Shapes => new() { "FromCombined", "FromBlank", "FromHistory", "FromPatientExport", "FromProfileReport" };

    private static ReportDocumentContent Build(string shape, bool withTemplates) => shape switch
    {
        "FromCombined" => Combined(withTemplates),
        "FromBlank" => Blank(withTemplates),
        "FromHistory" => History(withTemplates),
        "FromPatientExport" => PatientExport(withTemplates),
        "FromProfileReport" => ProfileReport(withTemplates),
        _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, "Unknown report shape.")
    };

    [Theory]
    [MemberData(nameof(Shapes))]
    public void NullLayout_ProducesTheSameTextAsTheNoTemplatePath(string shape)
    {
        var withTemplates = Build(shape, withTemplates: true);
        var withoutTemplates = Build(shape, withTemplates: false);

        Assert.Equal(withoutTemplates.BuildDisplayLines(), withTemplates.BuildDisplayLines());
    }

    [Theory]
    [MemberData(nameof(Shapes))]
    public void NullLayout_LeavesEveryLineWithoutPerItemLayout(string shape)
    {
        var content = Build(shape, withTemplates: true);

        var lines = content.Sections.SelectMany(s => s.LayoutLines).ToList();
        Assert.All(lines, line => Assert.False(line.HasLayout));
        Assert.All(content.Sections, s => Assert.True(s.HeadingLine is null || !s.HeadingLine.HasLayout));
    }

    [Theory]
    [MemberData(nameof(Shapes))]
    public void NullLayout_ResolvesToAnIdentityComposerLayout(string shape)
    {
        var settings = ReportSettings.CreateDefault();
        var content = Build(shape, withTemplates: true);

        foreach (var line in content.Sections.SelectMany(s => s.LayoutLines))
        {
            var layout = ReportPageComposer.ResolveItemLayout(line, settings);
            Assert.Equal(line.Text, layout.Text);
            Assert.Null(layout.LineHeight);
            Assert.Equal(0f, layout.PaddingLeftPt);
            Assert.Equal(settings.PageMarginLeftCm, layout.LeftPositionCm);
        }
    }

    [Theory]
    [MemberData(nameof(Shapes))]
    public void NullLayout_RendersWithoutFailing(string shape)
    {
        var content = Build(shape, withTemplates: true);
        var path = Path.Combine(_dir, $"{shape}-{Guid.NewGuid():N}.pdf");

        new ReportDocument(content, ReportSettings.CreateDefault()).WriteToFile(path);

        Assert.True(new FileInfo(path).Length > 0);
    }
}