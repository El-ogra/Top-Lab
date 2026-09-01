using FluentValidation;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("معرف المستخدم مطلوب.");
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("اسم المستخدم مطلوب.")
            .MaximumLength(100).WithMessage("اسم المستخدم يجب ألا يتجاوز 100 حرف.");

        RuleFor(x => x.DiscountLimitPercent)
            .InclusiveBetween(0, 100).WithMessage("نسبة الخصم يجب أن تكون بين 0 و 100.");

        RuleFor(x => x.Password)
            .Must(p => string.IsNullOrEmpty(p) || p.Length >= 6).WithMessage("كلمة المرور يجب ألا تقل عن 6 أحرف عند تزويدها.")
            .Must(p => string.IsNullOrEmpty(p) || !string.IsNullOrWhiteSpace(p)).WithMessage("كلمة المرور غير صالحة.");

        RuleFor(x => x.SecondaryPassword)
            .Must(p => string.IsNullOrEmpty(p) || p.Length >= 6).WithMessage("كلمة المرور الثانوية يجب ألا تقل عن 6 أحرف عند تزويدها.")
            .Must(p => string.IsNullOrEmpty(p) || !string.IsNullOrWhiteSpace(p)).WithMessage("كلمة المرور الثانوية غير صالحة.");

        RuleFor(x => x)
            .Must(HaveValidWorkingHours).WithMessage("ساعات العمل يجب أن تكون كلاهما فارغًا أو كلاهما محددًا وبداية العمل قبل النهاية.");

        RuleFor(x => x)
            .Must(HaveValidBreak).WithMessage("مدة الاستراحة يجب أن تكون موجبة عند تفعيل الاستراحة.");
    }

    private static bool HaveValidWorkingHours(UpdateUserCommand cmd)
    {
        if (cmd.WorkStartTime is null && cmd.WorkEndTime is null) return true;
        if (cmd.WorkStartTime is null || cmd.WorkEndTime is null) return false;
        return cmd.WorkStartTime.Value < cmd.WorkEndTime.Value;
    }

    private static bool HaveValidBreak(UpdateUserCommand cmd)
    {
        if (cmd.HasBreakPeriod)
        {
            return cmd.BreakDurationMinutes.HasValue && cmd.BreakDurationMinutes.Value > 0;
        }

        return true;
    }
}
