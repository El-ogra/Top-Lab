using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for PatientTitleId.</summary>
public sealed class PatientTitleId : StronglyTypedId<int>
{
    private PatientTitleId(int value) : base(value)
    {
    }

    public static PatientTitleId Create(int value) => new(value);
}
