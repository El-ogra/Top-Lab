using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Commands.UpdatePatient;

public sealed class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرف المريض غير صالح.");
        RuleFor(x => x.FullName).NotEmpty().WithMessage("الاسم الكامل مطلوب.");
        RuleFor(x => x.AgeValue).GreaterThanOrEqualTo(0).WithMessage("العمر يجب أن يكون صفرًا أو أكبر.");
    }
}
