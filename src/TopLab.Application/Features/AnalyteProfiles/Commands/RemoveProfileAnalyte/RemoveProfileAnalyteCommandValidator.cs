using FluentValidation;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.RemoveProfileAnalyte;

public sealed class RemoveProfileAnalyteCommandValidator : AbstractValidator<RemoveProfileAnalyteCommand>
{
    public RemoveProfileAnalyteCommandValidator()
    {
        RuleFor(x => x.ProfileId).GreaterThan(0).WithMessage("معرف البروفايل غير صالح.");
        RuleFor(x => x.AnalyteId).GreaterThan(0).WithMessage("معرف المادة التحليلية غير صالح.");
    }
}
