using FluentValidation;

namespace TopLab.Application.Features.ResultDelivery.Commands.PrintGroupPatientSlips;

public sealed class PrintGroupPatientSlipsCommandValidator : AbstractValidator<PrintGroupPatientSlipsCommand>
{
    public PrintGroupPatientSlipsCommandValidator()
    {
        RuleFor(x => x.PatientIds)
            .NotEmpty().WithMessage("قائمة المرضى مطلوبة.");

        RuleForEach(x => x.PatientIds)
            .GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
