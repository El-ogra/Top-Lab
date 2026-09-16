using FluentValidation;

namespace TopLab.Application.Features.WorkSheets.Commands.PrintWorkSheet;

public sealed class PrintWorkSheetCommandValidator : AbstractValidator<PrintWorkSheetCommand>
{
    public PrintWorkSheetCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
