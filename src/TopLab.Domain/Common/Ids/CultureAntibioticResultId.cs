using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for CultureAntibioticResultId.</summary>
public sealed class CultureAntibioticResultId : StronglyTypedId<int>
{
    private CultureAntibioticResultId(int value) : base(value)
    {
    }

    public static CultureAntibioticResultId Create(int value) => new(value);
}
