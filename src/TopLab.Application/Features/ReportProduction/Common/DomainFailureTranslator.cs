namespace TopLab.Application.Features.ReportProduction.Common;

internal static class DomainFailureTranslator
{
    internal static string Translate(ArgumentException ex)
    {
        return ex.ParamName switch
        {
            "isReviewed" => "لا يمكن إدراج نتيجة غير معتمدة في التقرير.",
            "patientTestId" => "لا يمكن تكرار نفس التحليل في التقرير.",
            "labId" or "fullName" => "تعذر تحديد هوية المريض للتاريخ المرضي.",
            _ => "بيانات غير صالحة."
        };
    }
}