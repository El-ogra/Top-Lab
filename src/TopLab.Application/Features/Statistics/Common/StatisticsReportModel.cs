using TopLab.Application.Features.Statistics.Common;

namespace TopLab.Application.Features.Statistics.Common;

/// <summary>
/// GAP-18 (5.6.2.3) — pure, QuestPDF-free document model for the on-screen census
/// report. The builder is deterministic and side-effect free so tests can prove the saved
/// artefact mirrors the preview.
/// </summary>
public sealed record StatisticsReportSections(
    bool BySex,
    bool ByReferralEntity,
    bool ByAccountType,
    bool GroupByMonth,
    bool GroupByDayOfMonth,
    bool IncludeMoneyRow);

public sealed record StatisticsReportRow(string Label, int Count);

public sealed record StatisticsReportSection(string Title, IReadOnlyList<StatisticsReportRow> Rows);

public sealed record StatisticsReportModel(
    DateOnly From,
    DateOnly To,
    int TotalCount,
    PeriodMoneyDto? Money,
    IReadOnlyList<StatisticsReportSection> Sections);

public static class StatisticsReportModelBuilder
{
    public static StatisticsReportModel Build(PatientCountStatisticsDto report, StatisticsReportSections sections)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(sections);

        var list = new List<StatisticsReportSection>();

        if (sections.BySex)
        {
            list.Add(new StatisticsReportSection(
                "حسب الجنس",
                report.SexCounts.Select(c => new StatisticsReportRow(c.DisplayName, c.Count)).ToList()));
        }

        if (sections.ByReferralEntity)
        {
            list.Add(new StatisticsReportSection(
                "حسب جهة الإحالة",
                report.ReferralEntityCounts.Select(c => new StatisticsReportRow(c.DisplayName, c.Count)).ToList()));
        }

        if (sections.ByAccountType)
        {
            list.Add(new StatisticsReportSection(
                "حسب نوع الحساب",
                report.AccountTypeCounts.Select(c => new StatisticsReportRow(c.DisplayName, c.Count)).ToList()));
        }

        if (sections.GroupByMonth)
        {
            list.Add(new StatisticsReportSection(
                "تجميع شهري",
                report.MonthlyCounts.Select(c => new StatisticsReportRow($"{c.Year}/{c.Month:D2}", c.Count)).ToList()));
        }

        if (sections.GroupByDayOfMonth)
        {
            list.Add(new StatisticsReportSection(
                "حسب يوم الشهر",
                report.DayOfMonthCounts.Select(c => new StatisticsReportRow(c.Day.ToString(System.Globalization.CultureInfo.InvariantCulture), c.Count)).ToList()));
        }

        return new StatisticsReportModel(
            report.From,
            report.To,
            report.TotalCount,
            sections.IncludeMoneyRow ? report.Money : null,
            list);
    }
}