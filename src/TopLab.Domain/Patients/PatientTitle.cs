using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Patients;

public sealed class PatientTitle : Entity<PatientTitleId>
{
    public string TitleText { get; private set; } = default!;

    public bool IsDefault { get; private set; }

    private PatientTitle()
    {
    }

    private PatientTitle(PatientTitleId id, string titleText, bool isDefault)
        : base(id)
    {
        TitleText = titleText;
        IsDefault = isDefault;
    }

    public static PatientTitle Create(PatientTitleId id, string titleText, bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(titleText))
        {
            throw new ArgumentException("TitleText is required.", nameof(titleText));
        }

        return new PatientTitle(id, titleText.Trim(), isDefault);
    }
}
