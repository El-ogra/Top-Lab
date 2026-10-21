using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Patients;

public sealed class MedicalConditionType : Entity<MedicalConditionTypeId>
{
    public string Name { get; private set; } = default!;

    public MedicalConditionCategory Category { get; private set; }

    private MedicalConditionType()
    {
    }

    private MedicalConditionType(MedicalConditionTypeId id, string name, MedicalConditionCategory category)
        : base(id)
    {
        Name = name;
        Category = category;
    }

    public static MedicalConditionType Create(MedicalConditionTypeId id, string name, MedicalConditionCategory category)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        return new MedicalConditionType(id, name.Trim(), category);
    }

    /// <summary>
    /// STAGE-01 (baseline 1.2.2.3): renames a catalogue row from the
    /// maintenance screen. The category is immutable — it drives the pregnancy
    /// signal and the block's grouping.
    /// </summary>
    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        Name = name.Trim();
    }
}
