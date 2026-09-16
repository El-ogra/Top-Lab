using FluentValidation;

namespace TopLab.Application.Features.Utilities.Commands.AddPhoneBookEntry;

public sealed class AddPhoneBookEntryCommandValidator : AbstractValidator<AddPhoneBookEntryCommand>
{
    public AddPhoneBookEntryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("الاسم مطلوب.")
            .MaximumLength(200);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("رقم الهاتف مطلوب.")
            .MaximumLength(30);

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => x.Notes is not null);
    }
}
