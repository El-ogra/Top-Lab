using FluentValidation;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateSystemSettings;

public sealed class UpdateSystemSettingsCommandValidator : AbstractValidator<UpdateSystemSettingsCommand>
{
    public UpdateSystemSettingsCommandValidator()
    {
        RuleFor(x => x.DefaultAccountType)
            .Must(IsAllowedDefaultAccountType).WithMessage("نوع الحساب الافتراضي غير صالح.");

        RuleFor(x => x.DailyBackupPath)
            .Must((cmd, path) => !cmd.DailyBackupEnabled || !string.IsNullOrWhiteSpace(path))
            .WithMessage("مسار النسخ الاحتياطي مطلوب عند تفعيل النسخ الاحتياطي اليومي.")
            .MaximumLength(300).WithMessage("مسار النسخ الاحتياطي يجب ألا يتجاوز 300 حرف.");
    }

    private static bool IsAllowedDefaultAccountType(AccountType value)
    {
        return value == AccountType.Individual
            || value == AccountType.LabToLab
            || value == AccountType.Contracts
            || value == AccountType.Free;
    }
}