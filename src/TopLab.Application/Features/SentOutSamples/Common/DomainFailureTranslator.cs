namespace TopLab.Application.Features.SentOutSamples.Common;

/// <summary>
/// Maps S1 Domain <see cref="ArgumentException"/> param-names to the
/// Appendix A Arabic messages (M-13/14/15 translator precedent).
/// Domain remains authoritative at runtime; validators mirror textually.
/// </summary>
internal static class DomainFailureTranslator
{
    internal static string Translate(ArgumentException ex)
    {
        if (ex.ParamName == "costPrice" || ex.ParamName == "patientPrice")
        {
            return "السعر يجب ألا يكون سالبًا.";
        }

        if (ex.ParamName == "amountPaid")
        {
            return "مبلغ الدفع يجب أن يكون أكبر من صفر.";
        }

        return "بيانات غير صالحة.";
    }
}
