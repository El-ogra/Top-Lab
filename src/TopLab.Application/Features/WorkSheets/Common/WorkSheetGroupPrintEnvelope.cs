using System.Text.Json;
using TopLab.Application.Features.WorkSheets.Common;

namespace TopLab.Application.Features.WorkSheets.Common;

/// <summary>
/// GAP-17: self-contained test-group worksheet print payload handed to
/// <c>IWorkSheetPrintingService</c> through its single string token — the
/// JSON-serialized <c>WorkSheetDto</c> produced by
/// <c>GetWorkSheetByTestGroupQuery</c>. Mirrors <see cref="WorkSheetPrintEnvelope"/>.
/// </summary>
public sealed record WorkSheetGroupPrintEnvelope(string WorkSheetJson)
{
    public static string CreateToken(WorkSheetDto sheet)
    {
        ArgumentNullException.ThrowIfNull(sheet);
        return JsonSerializer.Serialize(new WorkSheetGroupPrintEnvelope(JsonSerializer.Serialize(sheet)));
    }
}
