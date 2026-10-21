namespace TopLab.Application.Features.PatientRegistration.Common;

/// <summary>
/// STAGE-01 (baseline 1.2.2.3): the medical-history catalogue's baseline
/// presentation order and the identifiers that belong to the migration's seed.
/// </summary>
/// <remarks>
/// The block shows the questions in the baseline's sequence: the fasting pair,
/// then the six medication toggles, then the pregnancy toggle, then the single
/// imaging question, then the five condition toggles. The catalogue query's
/// <c>Category, Name</c> ordering does not produce that sequence, so the order
/// is applied explicitly here.
/// </remarks>
public static class MedicalConditionCatalogue
{
    /// <summary>
    /// The seeded identifiers of the catalogue (the existing pregnancy row plus
    /// the eleven rows added by STAGE-01). Seeded rows are read-only in the
    /// maintenance screen so that the catalogue cannot drift from the
    /// migration's <c>HasData</c> snapshot.
    /// </summary>
    public static readonly IReadOnlySet<int> SeededIds =
        new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

    /// <summary>
    /// The baseline question order of the seeded rows: the six medication
    /// toggles, the pregnancy toggle, then the five condition toggles.
    /// </summary>
    public static readonly int[] BaselineOrder =
        { 2, 3, 4, 5, 6, 7, 1, 8, 9, 10, 11, 12 };

    /// <summary>True when the identifier belongs to the seeded catalogue.</summary>
    public static bool IsSeeded(int medicalConditionTypeId) => SeededIds.Contains(medicalConditionTypeId);

    /// <summary>
    /// The position of a catalogue row in the baseline's question order.
    /// Operator-added rows rank after every seeded row.
    /// </summary>
    public static int BaselineRank(int medicalConditionTypeId)
    {
        var index = Array.IndexOf(BaselineOrder, medicalConditionTypeId);
        return index >= 0 ? index : int.MaxValue;
    }
}
