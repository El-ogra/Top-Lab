using FluentValidation;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateDatabaseServerSettings;

public sealed class UpdateDatabaseServerSettingsCommandValidator : AbstractValidator<UpdateDatabaseServerSettingsCommand>
{
    public UpdateDatabaseServerSettingsCommandValidator()
    {
        RuleFor(x => x.Server).NotEmpty().WithMessage("عنوان الخادم مطلوب.");
        RuleFor(x => x.Database).NotEmpty().WithMessage("اسم قاعدة البيانات مطلوب.");
    }
}
