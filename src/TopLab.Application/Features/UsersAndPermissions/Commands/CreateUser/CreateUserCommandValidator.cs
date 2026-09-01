using FluentValidation;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("اسم المستخدم مطلوب.")
            .MaximumLength(100).WithMessage("اسم المستخدم يجب ألا يتجاوز 100 حرف.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة.")
            .MinimumLength(6).WithMessage("كلمة المرور يجب ألا تقل عن 6 أحرف.")
            .Must(p => !string.IsNullOrWhiteSpace(p)).WithMessage("كلمة المرور مطلوبة.");

        RuleFor(x => x.SecondaryPassword)
            .NotEmpty().WithMessage("كلمة المرور الثانوية مطلوبة.")
            .MinimumLength(6).WithMessage("كلمة المرور الثانوية يجب ألا تقل عن 6 أحرف.")
            .Must(p => !string.IsNullOrWhiteSpace(p)).WithMessage("كلمة المرور الثانوية مطلوبة.");

        RuleFor(x => x.DiscountLimitPercent)
            .InclusiveBetween(0, 100).WithMessage("نسبة الخصم يجب أن تكون بين 0 و 100.");

        RuleFor(x => x)
            .Must(HaveValidWorkingHours).WithMessage("ساعات العمل يجب أن تكون كلاهما فارغًا أو كلاهما محددًا وبداية العمل قبل النهاية.");

        RuleFor(x => x)
            .Must(HaveValidBreak).WithMessage("مدة الاستراحة يجب أن تكون موجبة عند تفعيل الاستراحة.");
    }

    private static bool HaveValidWorkingHours(CreateUserCommand cmd)
    {
        if (cmd.WorkStartTime is null && cmd.WorkEndTime is null) return true;
        if (cmd.WorkStartTime is null || cmd.WorkEndTime is null) return false;
        return cmd.WorkStartTime.Value < cmd.WorkEndTime.Value;
    }

    private static bool HaveValidBreak(CreateUserCommand cmd)
    {
        if (cmd.HasBreakPeriod)
        {
            return cmd.BreakDurationMinutes.HasValue && cmd.BreakDurationMinutes.Value > 0;
        }

        return true;
    }
}
