using FluentValidation;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.SaveLabPrintText;

public sealed class SaveLabPrintTextCommandValidator : AbstractValidator<SaveLabPrintTextCommand>
{
    public SaveLabPrintTextCommandValidator()
    {
        RuleFor(x => x.LabName)
            .NotEmpty().WithMessage("اسم المعمل مطلوب.")
            .MaximumLength(200).WithMessage("اسم المعمل يجب ألا يتجاوز 200 حرف.");

        RuleFor(x => x.FontSizePt)
            .GreaterThan(0).WithMessage("حجم الخط يجب أن يكون أكبر من صفر.")
            .LessThanOrEqualTo(300).WithMessage("حجم الخط يجب ألا يتجاوز 300 نقطة.");
    }
}