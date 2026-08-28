using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for MedicalConditionTypeId.</summary>
public sealed class MedicalConditionTypeId : StronglyTypedId<int>
{
    private MedicalConditionTypeId(int value) : base(value)
    {
    }

    public static MedicalConditionTypeId Create(int value) => new(value);
}
