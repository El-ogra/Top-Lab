namespace TopLab.Application.Features.PatientBilling.Common;

internal static class DomainFailureTranslator
{
    internal static string Translate(ArgumentException ex)
    {
        if (ex.ParamName == "amount")
        {
            return ex.Message.Contains("Settlement")
                ? "مبلغ التسوية يجب أن يكون أكبر من صفر."
                : "المبلغ يجب أن يكون صفرًا أو أكثر.";
        }

        if (ex.ParamName == "discountAmount")
        {
            return ex.Message.Contains("extra charge")
                ? "لا يمكن إضافة خصم على مبلغ إضافي."
                : "الخصم يجب أن يكون صفرًا أو أكثر ولا يتجاوز مبلغ العملية.";
        }

        return "بيانات غير صالحة.";
    }
}
