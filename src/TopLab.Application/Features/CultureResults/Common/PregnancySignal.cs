using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.CultureResults.Common;

internal static class PregnancySignal
{
    public static bool IsPregnancyIndicated(IEnumerable<MedicalConditionCategory> attachedConditionCategories) =>
        attachedConditionCategories.Any(category => category == MedicalConditionCategory.Pregnancy);
}
