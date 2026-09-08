using FluentValidation;
using TopLab.Application.Features.PatientRegistration.Common;

namespace TopLab.Application.Features.PatientRegistration.Commands.CreatePatient;

public sealed class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("اسم المريض مطلوب.")
            .MaximumLength(200).WithMessage("اسم المريض يجب ألا يتجاوز 200 حرفًا.");

        RuleFor(x => x.AgeValue)
            .GreaterThanOrEqualTo(0).WithMessage("العمر يجب أن يكون صفرًا أو أكثر.");

        RuleFor(x => x.RegistrationDateUtc)
            .NotEqual(default(DateTime)).WithMessage("تاريخ التسجيل مطلوب.");

        RuleFor(x => x.PickupDateUtc)
            .GreaterThan(x => x.RegistrationDateUtc).When(x => x.PickupDateUtc.HasValue)
            .WithMessage("تاريخ الاستلام يجب أن يكون بعد تاريخ registration.");

        When(x => x.IsFastingIndicated, () =>
        {
            RuleFor(x => x.FastingHours!.Value)
                .GreaterThan(0).WithMessage("ساعات الصيام يجب أن تكون أكبر من صفر.");
        }).Otherwise(() =>
        {
            RuleFor(x => x.FastingHours!.Value)
                .Equal(0).When(x => x.FastingHours.HasValue)
                .WithMessage("ساعات الصيام تتطلب تحديد الصيام.");
        });

        RuleFor(x => x.MedicalConditionIds)
            .Must(ids => ids == null || ids.All(id => id > 0))
            .WithMessage("معرّف نوع الحالة الصحية غير صالح.");

        RuleForEach(x => x.PhoneNumbers)
            .ChildRules(phone =>
            {
                phone.RuleFor(p => p.Number)
                    .NotEmpty().WithMessage("رقم الهاتف مطلوب.")
                    .MaximumLength(30).WithMessage("رقم الهاتف يجب ألا يتجاوز 30 حرفًا.");
            });

        RuleFor(x => x.Tests)
            .NotEmpty().WithMessage("يجب اختيار تحليل واحد على الأقل.");

        RuleForEach(x => x.Tests).ChildRules(test =>
        {
            test.RuleFor(t => t.TestId).GreaterThan(0).WithMessage("معرّف التحليل غير صالح.");
        });

        RuleFor(x => x.Tests)
            .Must(t => t == null || t.Select(x => x.TestId).Distinct().Count() == t.Count)
            .When(x => x.Tests != null && x.Tests.Count > 0)
            .WithMessage("لا يمكن تكرار نفس التحليل في نفس الزيارة.");
    }
}