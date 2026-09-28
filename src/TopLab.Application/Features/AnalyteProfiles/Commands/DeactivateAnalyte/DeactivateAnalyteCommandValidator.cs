using FluentValidation;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.DeactivateAnalyte;

public sealed class DeactivateAnalyteCommandValidator : AbstractValidator<DeactivateAnalyteCommand>
{
    public DeactivateAnalyteCommandValidator()
    {
        RuleFor(x => x.AnalyteId).GreaterThan(0).WithMessage("معرف المادة التحليلية غير صالح.");
    }
}
