using System.Text.Json;
using TopLab.Application.Features.WorkSheets.Common;

namespace TopLab.Application.Features.WorkSheets.Common;

/// <summary>
/// Self-contained visit-worksheet print payload handed to
/// <c>IWorkSheetPrintingService</c> through its single string token: the
/// JSON-serialized <c>VisitWorkSheetDto</c> produced by
/// <c>GetVisitWorkSheetQuery</c>. Mirrors <c>ReceiptPrintEnvelope</c>.
/// </summary>
public sealed record WorkSheetPrintEnvelope(string WorkSheetJson)
{
    public static string CreateToken(VisitWorkSheetDto sheet)
    {
        ArgumentNullException.ThrowIfNull(sheet);
        return JsonSerializer.Serialize(new WorkSheetPrintEnvelope(JsonSerializer.Serialize(sheet)));
    }
}
