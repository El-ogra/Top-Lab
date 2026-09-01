using FluentValidation;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.SaveUserPermissions;

public sealed class SaveUserPermissionsCommandValidator : AbstractValidator<SaveUserPermissionsCommand>
{
    public SaveUserPermissionsCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("معرف المستخدم مطلوب.");
        RuleFor(x => x.PermissionCodes).NotNull().WithMessage("قائمة الصلاحيات مطلوبة.");
    }
}
