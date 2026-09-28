using FluentValidation;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.BackupDatabaseNow;

public sealed class BackupDatabaseNowCommandValidator : AbstractValidator<BackupDatabaseNowCommand>
{
    public BackupDatabaseNowCommandValidator()
    {
        RuleFor(x => x.DestinationDirectory).NotEmpty().WithMessage("مسار النسخة الاحتياطية مطلوب.");
    }
}
