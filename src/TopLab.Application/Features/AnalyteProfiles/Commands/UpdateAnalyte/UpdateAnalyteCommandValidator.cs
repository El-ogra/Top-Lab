using FluentValidation;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.UpdateAnalyte;

public sealed class UpdateAnalyteCommandValidator : AbstractValidator<UpdateAnalyteCommand>
{
    public UpdateAnalyteCommandValidator()
    {
        RuleFor(x => x.AnalyteId).GreaterThan(0).WithMessage("معرف المادة التحليلية غير صالح.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("الاسم مطلوب.");
    }
}
