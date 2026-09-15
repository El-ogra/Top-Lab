namespace TopLab.Application.Features.Statistics.Common;

/// <summary>
/// Permission code consumed by the M-19 statistics queries.
/// All four read surfaces are gated on STATISTICS (seeded id=12, FR-M17-004 item 12);
/// absolute-permission users bypass via the existing pipeline.
/// </summary>
public static class StatisticsAccessPolicy
{
    public const string Statistics = "STATISTICS";
}
