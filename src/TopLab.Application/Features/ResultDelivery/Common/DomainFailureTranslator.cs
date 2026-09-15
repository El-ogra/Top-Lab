namespace TopLab.Application.Features.ResultDelivery.Common;

/// <summary>
/// Translates the delivery Domain guard rejection into the frozen M-09 Arabic
/// message (Appendix A). The Domain guard <c>PatientTest.MarkDelivered</c> is
/// authoritative; this mapping only renders its rejection.
/// </summary>
internal static class DomainFailureTranslator
{
    internal static string Translate(InvalidOperationException ex)
    {
        return ex.Message switch
        {
            "Result not printed." => "النتيجة غير مطبوعة.",
            _ => "بيانات غير صالحة."
        };
    }
}
