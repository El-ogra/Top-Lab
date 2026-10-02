using FluentValidation;

namespace TopLab.Application.Features.CultureAndAntibiotics.Commands.CreateAntibiotic;

public sealed class CreateAntibioticCommandValidator : AbstractValidator<CreateAntibioticCommand>
{
    public CreateAntibioticCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المضاد الحيوي مطلوب.")
            .MaximumLength(150).WithMessage("اسم المضاد الحيوي يجب ألا يتجاوز 150 حرفًا.");
        RuleFor(x => x.Symbol)
            .MaximumLength(10).WithMessage("الرمز يجب ألا يتجاوز 10 أحرف.");
        RuleFor(x => x.ScientificName)
            .MaximumLength(150).WithMessage("الاسم العلمي يجب ألا يتجاوز 150 حرفًا.");
    }
}