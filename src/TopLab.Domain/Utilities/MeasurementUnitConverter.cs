namespace TopLab.Domain.Utilities;

/// <summary>
/// Pure static Domain service for laboratory unit conversion (SD-23-8).
/// Fixed explicit conversion-pair table; unknown pairs throw — never a silent identity conversion.
/// </summary>
public static class MeasurementUnitConverter
{
    private static readonly Dictionary<string, decimal> Factors = new(StringComparer.OrdinalIgnoreCase)
    {
        // Mass (metric prefixes)
        ["g->mg"] = 1000m,
        ["mg->g"] = 0.001m,
        ["g->µg"] = 1_000_000m,
        ["µg->g"] = 0.000001m,
        ["mg->µg"] = 1000m,
        ["µg->mg"] = 0.001m,
        ["kg->g"] = 1000m,
        ["g->kg"] = 0.001m,

        // Concentration / amount pairs documented for lab use
        ["g/L->mg/dL"] = 100m,
        ["mg/dL->g/L"] = 0.01m,
        ["mmol/L->µmol/L"] = 1000m,
        ["µmol/L->mmol/L"] = 0.001m,
        ["ng/mL->µg/L"] = 1m,
        ["µg/L->ng/mL"] = 1m,

        // Volume
        ["L->mL"] = 1000m,
        ["mL->L"] = 0.001m,
        ["mL->µL"] = 1000m,
        ["µL->mL"] = 0.001m,
    };

    /// <summary>Converts <paramref name="value"/> from <paramref name="fromUnit"/> to <paramref name="toUnit"/>.</summary>
    /// <exception cref="ArgumentException">Unknown conversion pair or blank unit.</exception>
    public static decimal Convert(decimal value, string fromUnit, string toUnit)
    {
        if (string.IsNullOrWhiteSpace(fromUnit) || string.IsNullOrWhiteSpace(toUnit))
        {
            throw new ArgumentException("Unit is required.", nameof(fromUnit));
        }

        var from = fromUnit.Trim();
        var to = toUnit.Trim();

        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        var key = $"{from}->{to}";
        if (!Factors.TryGetValue(key, out var factor))
        {
            throw new ArgumentException($"Unsupported unit pair '{from}' -> '{to}'.", nameof(fromUnit));
        }

        return value * factor;
    }
}
