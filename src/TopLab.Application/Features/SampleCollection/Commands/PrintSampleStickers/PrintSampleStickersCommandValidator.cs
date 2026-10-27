using FluentValidation;

namespace TopLab.Application.Features.SampleCollection.Commands.PrintSampleStickers;

public sealed class PrintSampleStickersCommandValidator : AbstractValidator<PrintSampleStickersCommand>
{
    public PrintSampleStickersCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرف غير صالح.");
    }
}