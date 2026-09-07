namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;

internal static class DomainFailureTranslator
{
    internal static string Translate(ArgumentException ex)
    {
        if (ex.ParamName == "name")
        {
            return ex.Message.Contains("at most")
                ? "الاسم يجب ألا يتجاوز 150 حرفًا."
                : "الاسم مطلوب.";
        }

        if (ex.ParamName == "commentText")
        {
            return ex.Message.Contains("at most")
                ? "نص التعليق يجب ألا يتجاوز 1000 حرف."
                : "نص التعليق مطلوب.";
        }

        if (ex.ParamName == "price")
        {
            return "السعر يجب أن يكون صفرًا أو أكثر.";
        }

        if (ex.ParamName == "testId")
        {
            return ex.Message.Contains("custom group")
                ? "التحليل غير موجود في المجموعة."
                : "التحليل غير موجود في القائمة.";
        }

        return "بيانات غير صالحة.";
    }
}
