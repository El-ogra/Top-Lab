using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class SystemSettingsConfiguration : IEntityTypeConfiguration<SystemSettings>
{
    public void Configure(EntityTypeBuilder<SystemSettings> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).ValueGeneratedNever().HasColumnName("SystemSettingsId");
        b.Property(e => e.DefaultAccountType).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.PrintLabIdInsteadOfPatientId).IsRequired();
        b.Property(e => e.AutoReviewAndComplete).IsRequired();
        b.Property(e => e.ResultScreenAccountDisplayMode).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.SaveTreatingDoctorOnlyFromEntityWindow).IsRequired();
        b.Property(e => e.EnablePatientNameSearchAssist).IsRequired();
        b.Property(e => e.DisableAutoTitleInsertion).IsRequired();
        b.Property(e => e.PrintFileExternalBarcode).IsRequired();
        b.Property(e => e.PrintDateTimeOnTubeBarcode).IsRequired();
        b.Property(e => e.PrintAccountInsteadOfDateOnReport).IsRequired();
        b.Property(e => e.DailyBackupEnabled).IsRequired();
        b.Property(e => e.DailyBackupPath).HasMaxLength(300).IsRequired(false);
        b.HasData(new { Id = 1, DefaultAccountType = AccountType.Individual, PrintLabIdInsteadOfPatientId = false, AutoReviewAndComplete = false, ResultScreenAccountDisplayMode = ResultScreenAccountDisplayMode.Hidden, SaveTreatingDoctorOnlyFromEntityWindow = false, EnablePatientNameSearchAssist = false, DisableAutoTitleInsertion = false, PrintFileExternalBarcode = false, PrintDateTimeOnTubeBarcode = false, PrintAccountInsteadOfDateOnReport = false, DailyBackupEnabled = false, DailyBackupPath = (string?)null });
    }
}
