using FluentValidation;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportSettings;

public sealed class UpdateReportSettingsCommandValidator : AbstractValidator<UpdateReportSettingsCommand>
{
    public UpdateReportSettingsCommandValidator()
    {
        RuleFor(x => x.PageMarginLeftCm)
            .InclusiveBetween(0, 30).WithMessage("الهامش الأيسر يجب أن يكون بين 0 و 30 سم.");

        RuleFor(x => x.PageMarginBottomCm)
            .InclusiveBetween(0, 30).WithMessage("الهامش السفلي يجب أن يكون بين 0 و 30 سم.");

        RuleFor(x => x.ReportTopSpaceCm)
            .LessThanOrEqualTo(8m).WithMessage("الهامش العلوي للتقرير لا يمكن أن يتجاوز 8 سم")
            .GreaterThanOrEqualTo(0m).WithMessage("الهامش العلوي للتقرير يجب أن يكون أكبر من أو يساوي 0.");
    }
}