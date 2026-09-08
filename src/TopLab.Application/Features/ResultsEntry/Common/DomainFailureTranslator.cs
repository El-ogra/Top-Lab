namespace TopLab.Application.Features.ResultsEntry.Common;

internal static class DomainFailureTranslator
{
    internal static string Translate(InvalidOperationException ex)
    {
        return ex.Message switch
        {
            "Result is reviewed; unreview first." => "النتيجة معتمدة؛ ألغِ الاعتماد أولاً.",
            "Result is locked." => "لا يمكن مسح نتيجة معتمدة أو مطبوعة أو مسلمة.",
            "Printed or delivered results cannot be un-reviewed." => "لا يمكن إلغاء اعتماد نتيجة مطبوعة أو مسلمة.",
            "Result not entered." => "لا يمكن اعتماد نتيجة غير مدخلة.",
            "Result not reviewed." => "لا يمكن طباعة نتيجة غير معتمدة.",
            "Result not printed." => "لا يمكن تسليم نتيجة غير مطبوعة.",
            _ => "بيانات غير صالحة."
        };
    }
}
