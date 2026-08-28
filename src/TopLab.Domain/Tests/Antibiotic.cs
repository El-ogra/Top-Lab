using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

public sealed class Antibiotic : Entity<AntibioticId>
{
    public string Name { get; private set; } = default!;

    public bool IsPregnancyFlagged { get; private set; }

    public bool IsChildrenFlagged { get; private set; }

    private Antibiotic()
    {
    }

    private Antibiotic(AntibioticId id, string name, bool isPregnancyFlagged, bool isChildrenFlagged)
        : base(id)
    {
        Name = name;
        IsPregnancyFlagged = isPregnancyFlagged;
        IsChildrenFlagged = isChildrenFlagged;
    }

    public static Antibiotic Create(AntibioticId id, string name, bool isPregnancyFlagged = false, bool isChildrenFlagged = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        return new Antibiotic(id, name.Trim(), isPregnancyFlagged, isChildrenFlagged);
    }
}
