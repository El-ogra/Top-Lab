using TopLab.Application.Features.CultureResults.Common;
using TopLab.Domain.Common.Enums;
using Xunit;

namespace TopLab.Application.Tests.Features.CultureResults;

public class PregnancySignalTests
{
    [Fact]
    public void IsPregnancyIndicated_ReturnsTrue_WhenPregnancyCategoryIsAttached() =>
        Assert.True(PregnancySignal.IsPregnancyIndicated(new[]
        {
            MedicalConditionCategory.Condition,
            MedicalConditionCategory.Pregnancy
        }));

    [Fact]
    public void IsPregnancyIndicated_ReturnsFalse_WhenOnlyOtherCategoriesAreAttached() =>
        Assert.False(PregnancySignal.IsPregnancyIndicated(new[]
        {
            MedicalConditionCategory.Medication,
            MedicalConditionCategory.Condition
        }));

    [Fact]
    public void IsPregnancyIndicated_ReturnsFalse_WhenNoCategoriesAreAttached() =>
        Assert.False(PregnancySignal.IsPregnancyIndicated(Array.Empty<MedicalConditionCategory>()));
}
