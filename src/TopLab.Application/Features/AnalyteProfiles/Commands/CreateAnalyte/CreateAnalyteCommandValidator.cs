using FluentValidation;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.CreateAnalyte;

public sealed class CreateAnalyteCommandValidator : AbstractValidator<CreateAnalyteCommand>
{
    public CreateAnalyteCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المادة التحليلية مطلوب.")
            .MaximumLength(100);
        RuleFor(x => x.ReportName)
            .NotEmpty().WithMessage("اسم المادة التحليلية على التقرير مطلوب.")
            .MaximumLength(120);
    }
}