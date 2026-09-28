using FluentValidation;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.AddProfileAnalyte;

public sealed class AddProfileAnalyteCommandValidator : AbstractValidator<AddProfileAnalyteCommand>
{
    public AddProfileAnalyteCommandValidator()
    {
        RuleFor(x => x.ProfileId).GreaterThan(0).WithMessage("معرف البروفايل غير صالح.");
        RuleFor(x => x.AnalyteId).GreaterThan(0).WithMessage("معرف المادة التحليلية غير صالح.");
    }
}
