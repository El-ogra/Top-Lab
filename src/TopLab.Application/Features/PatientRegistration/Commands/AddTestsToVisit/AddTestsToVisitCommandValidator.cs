using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Commands.AddTestsToVisit;

public sealed class AddTestsToVisitCommandValidator : AbstractValidator<AddTestsToVisitCommand>
{
    public AddTestsToVisitCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
        RuleFor(x => x.Tests).NotEmpty().WithMessage("قائمة التحاليل مطلوبة.");
        RuleForEach(x => x.Tests).ChildRules(test =>
        {
            test.RuleFor(t => t.TestId).GreaterThan(0).WithMessage("معرّف التحليل غير صالح.");
        });

        RuleFor(x => x.Tests)
            .Must(t => t.Select(x => x.TestId).Distinct().Count() == t.Count)
            .WithMessage("لا يمكن تكرار نفس التحليل في نفس الزيارة.");
    }
}