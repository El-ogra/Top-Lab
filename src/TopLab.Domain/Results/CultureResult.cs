using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Results;

/// <summary>One-to-one with PatientTest, PK is PatientTestId.</summary>
public sealed class CultureResult
{
    public PatientTestId PatientTestId { get; private set; } = default!;

    public string? Sample { get; private set; }

    public string? OrganismA { get; private set; }

    public string? OrganismB { get; private set; }

    public string? OrganismC { get; private set; }

    public string? CultureCondition { get; private set; }

    public string? ColonyCount { get; private set; }

    private CultureResult()
    {
    }

    public CultureResult(
        PatientTestId patientTestId,
        string? sample = null,
        string? organismA = null,
        string? organismB = null,
        string? organismC = null,
        string? cultureCondition = null,
        string? colonyCount = null)
    {
        PatientTestId = patientTestId;
        Update(sample, organismA, organismB, organismC, cultureCondition, colonyCount);
    }

    public void Update(
        string? sample,
        string? organismA,
        string? organismB,
        string? organismC,
        string? cultureCondition,
        string? colonyCount)
    {
        Sample = Normalize(sample);
        OrganismA = Normalize(organismA);
        OrganismB = Normalize(organismB);
        OrganismC = Normalize(organismC);
        CultureCondition = Normalize(cultureCondition);
        ColonyCount = Normalize(colonyCount);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
