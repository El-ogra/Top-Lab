namespace TopLab.Application.Features.CultureAndAntibiotics.Common;

internal static class DomainFailureTranslator
{
    internal static string Translate(ArgumentException ex)
    {
        if (ex.ParamName == "name")
        {
            return ex.Message.Contains("at most")
                ? "اسم المضاد الحيوي يجب ألا يتجاوز 150 حرفًا."
                : "اسم المضاد الحيوي مطلوب.";
        }

        return "بيانات غير صالحة.";
    }
}