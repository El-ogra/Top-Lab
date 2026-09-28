using FluentValidation;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.DeactivateUser;

public sealed class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("معرف المستخدم غير صالح.");
    }
}
