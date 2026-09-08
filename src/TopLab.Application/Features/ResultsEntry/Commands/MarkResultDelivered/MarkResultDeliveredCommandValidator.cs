using FluentValidation;

namespace TopLab.Application.Features.ResultsEntry.Commands.MarkResultDelivered;

public sealed class MarkResultDeliveredCommandValidator : AbstractValidator<MarkResultDeliveredCommand>
{
    public MarkResultDeliveredCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرّف التحليل غير صالح.");
    }
}
