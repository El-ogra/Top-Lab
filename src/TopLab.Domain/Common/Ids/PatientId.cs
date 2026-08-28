using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for PatientId.</summary>
public sealed class PatientId : StronglyTypedId<int>
{
    private PatientId(int value) : base(value)
    {
    }

    public static PatientId Create(int value) => new(value);
}
