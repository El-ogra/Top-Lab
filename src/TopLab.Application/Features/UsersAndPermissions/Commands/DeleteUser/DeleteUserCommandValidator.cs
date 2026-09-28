using FluentValidation;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.DeleteUser;

public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("معرف المستخدم غير صالح.");
    }
}
