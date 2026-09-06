namespace TopLab.Application.Features.ExternalEntities.Common;

internal static class DomainFailureTranslator
{
    internal static string Translate(ArgumentException ex)
    {
        if (ex.ParamName == "name")
        {
            return ex.Message.Contains("at most")
                ? "اسم الجهة الخارجية يجب ألا يتجاوز 200 حرفًا."
                : "اسم الجهة الخارجية مطلوب.";
        }

        if (ex.ParamName == "priceListId")
        {
            return ex.Message.Contains("must not have")
                ? "الطبيب المعالج لا يرتبط بقائمة أسعار."
                : "جهة الإحالة / التعاقد تتطلب قائمة أسعار.";
        }

        if (ex.ParamName == "discountOrCommissionPercent")
        {
            return "نسبة الخصم / العمولة يجب أن تكون بين 0 و 100.";
        }

        if (ex.ParamName == "code")
        {
            return "رمز الجهة غير صالح.";
        }

        return "بيانات الجهة الخارجية غير صالحة.";
    }
}
