using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for PatientTestId.</summary>
public sealed class PatientTestId : StronglyTypedId<int>
{
    private PatientTestId(int value) : base(value)
    {
    }

    public static PatientTestId Create(int value) => new(value);
}
