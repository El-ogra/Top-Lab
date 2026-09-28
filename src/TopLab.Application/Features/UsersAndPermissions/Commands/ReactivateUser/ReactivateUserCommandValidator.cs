using FluentValidation;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.ReactivateUser;

public sealed class ReactivateUserCommandValidator : AbstractValidator<ReactivateUserCommand>
{
    public ReactivateUserCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("معرف المستخدم غير صالح.");
    }
}
