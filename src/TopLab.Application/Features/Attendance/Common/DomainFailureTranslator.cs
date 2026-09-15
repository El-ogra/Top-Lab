namespace TopLab.Application.Features.Attendance.Common;

/// <summary>
/// Maps S1 Domain <see cref="InvalidOperationException"/> messages to the
/// Appendix A Arabic messages (M-13/14/15/16 translator precedent).
/// Domain remains authoritative at runtime; validators mirror textually.
/// </summary>
internal static class DomainFailureTranslator
{
    internal static string Translate(InvalidOperationException ex)
    {
        return ex.Message switch
        {
            "Break is already started." => "الاستراحة بدأت بالفعل.",
            "No open break to end." => "لا توجد استراحة مفتوحة.",
            "Cannot check out with an open break." => "أنهِ الاستراحة قبل تسجيل الانصراف.",
            "Already checked out." => "تم تسجيل الانصراف مسبقًا.",
            _ => "بيانات غير صالحة."
        };
    }
}
