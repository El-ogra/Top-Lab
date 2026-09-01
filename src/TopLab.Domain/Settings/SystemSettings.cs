using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;

namespace TopLab.Domain.Settings;

/// <summary>Single row, PK=1.</summary>
public sealed class SystemSettings : Entity<int>
{
    public AccountType DefaultAccountType { get; private set; }

    public bool PrintLabIdInsteadOfPatientId { get; private set; }

    public bool AutoReviewAndComplete { get; private set; }

    public ResultScreenAccountDisplayMode ResultScreenAccountDisplayMode { get; private set; }

    public bool SaveTreatingDoctorOnlyFromEntityWindow { get; private set; }

    public bool EnablePatientNameSearchAssist { get; private set; }

    public bool DisableAutoTitleInsertion { get; private set; }

    public bool PrintFileExternalBarcode { get; private set; }

    public bool PrintDateTimeOnTubeBarcode { get; private set; }

    public bool PrintAccountInsteadOfDateOnReport { get; private set; }

    public bool DailyBackupEnabled { get; private set; }

    public string? DailyBackupPath { get; private set; }

    private SystemSettings()
    {
    }

    private SystemSettings(int id) : base(id)
    {
    }

    public static SystemSettings CreateDefault()
    {
        return new SystemSettings(1)
        {
            DefaultAccountType = AccountType.Individual,
            PrintLabIdInsteadOfPatientId = false,
            AutoReviewAndComplete = false,
            ResultScreenAccountDisplayMode = ResultScreenAccountDisplayMode.Hidden,
            SaveTreatingDoctorOnlyFromEntityWindow = false,
            EnablePatientNameSearchAssist = false,
            DisableAutoTitleInsertion = false,
            PrintFileExternalBarcode = false,
            PrintDateTimeOnTubeBarcode = false,
            PrintAccountInsteadOfDateOnReport = false,
            DailyBackupEnabled = false,
            DailyBackupPath = null
        };
    }

    public void SetDefaultAccountType(AccountType value)
    {
        if (value != AccountType.Individual
            && value != AccountType.LabToLab
            && value != AccountType.Contracts
            && value != AccountType.Free)
        {
            throw new ArgumentException("Vip is not a valid default account type.", nameof(value));
        }

        DefaultAccountType = value;
    }

    public void SetGeneralFlags(
        bool saveTreatingDoctorOnlyFromEntityWindow,
        bool enablePatientNameSearchAssist,
        bool disableAutoTitleInsertion,
        bool printFileExternalBarcode,
        bool printDateTimeOnTubeBarcode,
        bool printLabIdInsteadOfPatientId,
        bool autoReviewAndComplete,
        bool printAccountInsteadOfDateOnReport)
    {
        SaveTreatingDoctorOnlyFromEntityWindow = saveTreatingDoctorOnlyFromEntityWindow;
        EnablePatientNameSearchAssist = enablePatientNameSearchAssist;
        DisableAutoTitleInsertion = disableAutoTitleInsertion;
        PrintFileExternalBarcode = printFileExternalBarcode;
        PrintDateTimeOnTubeBarcode = printDateTimeOnTubeBarcode;
        PrintLabIdInsteadOfPatientId = printLabIdInsteadOfPatientId;
        AutoReviewAndComplete = autoReviewAndComplete;
        PrintAccountInsteadOfDateOnReport = printAccountInsteadOfDateOnReport;
    }

    public void SetResultScreenAccountDisplayMode(ResultScreenAccountDisplayMode mode)
    {
        ResultScreenAccountDisplayMode = mode;
    }

    public void SetDailyBackup(bool enabled, string? path)
    {
        if (enabled)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Backup path is required when daily backup is enabled.", nameof(path));
            }

            if (path.Length > 300)
            {
                throw new ArgumentException("Backup path must be at most 300 characters.", nameof(path));
            }

            DailyBackupPath = path;
        }

        DailyBackupEnabled = enabled;
    }
}
