using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

public sealed class Antibiotic : Entity<AntibioticId>
{
    public string Name { get; private set; } = default!;

    public bool IsPregnancyFlagged { get; private set; }

    public bool IsChildrenFlagged { get; private set; }

    /// <summary>W-02 S10 (WP-14): short code, e.g. "AMC".</summary>
    public string? Symbol { get; private set; }

    /// <summary>W-02 S10 (WP-14): scientific name printed in the sensitivity table.</summary>
    public string? ScientificName { get; private set; }

    public const int MaxSymbolLength = 10;

    public const int MaxScientificNameLength = 150;

    private Antibiotic()
    {
    }

    private Antibiotic(AntibioticId id, string name, bool isPregnancyFlagged, bool isChildrenFlagged, string? symbol, string? scientificName)
        : base(id)
    {
        Name = name;
        IsPregnancyFlagged = isPregnancyFlagged;
        IsChildrenFlagged = isChildrenFlagged;
        Symbol = symbol;
        ScientificName = scientificName;
    }

    public static Antibiotic Create(AntibioticId id, string name, bool isPregnancyFlagged = false, bool isChildrenFlagged = false, string? symbol = null, string? scientificName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        return new Antibiotic(id, name.Trim(), isPregnancyFlagged, isChildrenFlagged, Normalize(symbol, MaxSymbolLength, nameof(symbol)), Normalize(scientificName, MaxScientificNameLength, nameof(scientificName)));
    }

    public void Update(string name, bool isPregnancyFlagged, bool isChildrenFlagged, string? symbol = null, string? scientificName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        Name = name.Trim();
        IsPregnancyFlagged = isPregnancyFlagged;
        IsChildrenFlagged = isChildrenFlagged;
        Symbol = Normalize(symbol, MaxSymbolLength, nameof(symbol));
        ScientificName = Normalize(scientificName, MaxScientificNameLength, nameof(scientificName));
    }

    private static string? Normalize(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} must be at most {maxLength} characters.", paramName);
        }

        return trimmed;
    }
}
