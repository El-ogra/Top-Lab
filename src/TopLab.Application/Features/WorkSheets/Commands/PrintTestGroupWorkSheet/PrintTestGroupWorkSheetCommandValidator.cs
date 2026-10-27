using FluentValidation;
using TopLab.Application.Features.WorkSheets.Common;

namespace TopLab.Application.Features.WorkSheets.Commands.PrintTestGroupWorkSheet;

public sealed class PrintTestGroupWorkSheetCommandValidator
    : AbstractValidator<PrintTestGroupWorkSheetCommand>
{
    public PrintTestGroupWorkSheetCommandValidator()
    {
        RuleFor(x => x.TestGroupId)
            .GreaterThan(0).WithMessage("معرف مجموعة التحاليل غير صالح.");

        RuleFor(x => x)
            .Must(c => WorkSheetPeriod.IsValid(c.From, c.To))
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
