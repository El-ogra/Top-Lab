using System.Text.Json;

namespace TopLab.Application.Features.ReportProduction.Common;

/// <summary>
/// Self-contained print payload handed to <c>IReportPrintingService</c> through its
/// single string token (M-07 plan R-1): the report kind plus the JSON-serialized
/// report DTO produced by the S2 builders/query. The Infrastructure service
/// deserializes the envelope, reads settings at print time, renders, and dispatches —
/// Application stays dependency-free and the port surface stays thin.
/// </summary>
public sealed record ReportPrintEnvelope(string ReportKind, string ReportJson)
{
    public const string Combined = "Combined";

    public const string Blank = "Blank";

    public const string History = "History";

    public static string CreateToken<T>(string kind, T report) where T : notnull
    {
        return JsonSerializer.Serialize(new ReportPrintEnvelope(kind, JsonSerializer.Serialize(report)));
    }
}