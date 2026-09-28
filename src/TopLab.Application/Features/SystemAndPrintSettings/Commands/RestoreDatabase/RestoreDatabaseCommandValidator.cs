using FluentValidation;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.RestoreDatabase;

public sealed class RestoreDatabaseCommandValidator : AbstractValidator<RestoreDatabaseCommand>
{
    public RestoreDatabaseCommandValidator()
    {
        RuleFor(x => x.BackupFilePath).NotEmpty().WithMessage("مسار ملف النسخة الاحتياطية مطلوب.");
    }
}
