namespace TopLab.Application.Features.ProfileResults.Common;

internal static class DomainFailureTranslator
{
    internal static string Translate(InvalidOperationException ex)
    {
        return ex.Message switch
        {
            "Result is reviewed; unreview first." => "النتيجة معتمدة؛ ألغِ الاعتماد أولاً.",
            "Result not entered." => "لا يمكن اعتماد نتيجة غير مدخلة.",
            "Result not reviewed." => "لا يمكن طباعة نتيجة غير معتمدة.",
            "Printed or delivered results cannot be un-reviewed." => "لا يمكن إلغاء اعتماد نتيجة مطبوعة أو مسلمة.",
            "Profile item not verified." => "المادة غير معتمدة؛ لا يمكن طباعتها.",
            "Printed profile items cannot be updated; use Amend." => "لا يمكن تعديل مادة مطبوعة إلا عبر مسار التعديل.",
            _ => "بيانات غير صالحة."
        };
    }
}