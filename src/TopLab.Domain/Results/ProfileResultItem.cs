using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Results;

public sealed class ProfileResultItem : Entity<ProfileResultItemId>
{
    public PatientTestId PatientTestId { get; private set; } = default!;

    public string AnalyteName { get; private set; } = default!;

    public string ResultValue { get; private set; } = default!;

    public string? Unit { get; private set; }

    public ProfileResultFlag? Flag { get; private set; }

    public bool IsVerified { get; private set; }

    public bool IsPrinted { get; private set; }

    private ProfileResultItem()
    {
    }

    private ProfileResultItem(
        ProfileResultItemId id,
        PatientTestId patientTestId,
        string analyteName,
        string resultValue,
        string? unit,
        ProfileResultFlag? flag,
        bool isVerified,
        bool isPrinted)
        : base(id)
    {
        PatientTestId = patientTestId;
        AnalyteName = analyteName;
        ResultValue = resultValue;
        Unit = unit;
        Flag = flag;
        IsVerified = isVerified;
        IsPrinted = isPrinted;
    }

    public static ProfileResultItem Create(
        ProfileResultItemId id,
        PatientTestId patientTestId,
        string analyteName,
        string resultValue,
        string? unit = null,
        ProfileResultFlag? flag = null,
        bool isVerified = false,
        bool isPrinted = false)
    {
        if (string.IsNullOrWhiteSpace(analyteName) || string.IsNullOrWhiteSpace(resultValue))
        {
            throw new ArgumentException("AnalyteName and ResultValue required.");
        }

        return new ProfileResultItem(id, patientTestId, analyteName.Trim(), resultValue.Trim(), unit, flag, isVerified, isPrinted);
    }
}
