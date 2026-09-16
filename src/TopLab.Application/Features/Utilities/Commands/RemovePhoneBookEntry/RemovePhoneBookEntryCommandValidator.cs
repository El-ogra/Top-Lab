using FluentValidation;

namespace TopLab.Application.Features.Utilities.Commands.RemovePhoneBookEntry;

public sealed class RemovePhoneBookEntryCommandValidator : AbstractValidator<RemovePhoneBookEntryCommand>
{
    public RemovePhoneBookEntryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
