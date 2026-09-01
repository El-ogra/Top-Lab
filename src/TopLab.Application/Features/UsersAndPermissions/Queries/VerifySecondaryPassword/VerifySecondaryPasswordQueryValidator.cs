using FluentValidation;

namespace TopLab.Application.Features.UsersAndPermissions.Queries.VerifySecondaryPassword;

public sealed class VerifySecondaryPasswordQueryValidator : AbstractValidator<VerifySecondaryPasswordQuery>
{
    public VerifySecondaryPasswordQueryValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("كلمة المرور مطلوبة.");
    }
}
