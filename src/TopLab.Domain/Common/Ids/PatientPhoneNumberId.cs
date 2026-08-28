using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for PatientPhoneNumberId.</summary>
public sealed class PatientPhoneNumberId : StronglyTypedId<int>
{
    private PatientPhoneNumberId(int value) : base(value)
    {
    }

    public static PatientPhoneNumberId Create(int value) => new(value);
}
