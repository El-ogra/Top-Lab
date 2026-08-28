using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for AntibioticId.</summary>
public sealed class AntibioticId : StronglyTypedId<int>
{
    private AntibioticId(int value) : base(value)
    {
    }

    public static AntibioticId Create(int value) => new(value);
}
