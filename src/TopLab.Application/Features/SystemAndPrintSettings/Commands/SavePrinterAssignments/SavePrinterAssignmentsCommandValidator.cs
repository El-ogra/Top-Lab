using FluentValidation;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.SavePrinterAssignments;

public sealed class SavePrinterAssignmentsCommandValidator : AbstractValidator<SavePrinterAssignmentsCommand>
{
    private static readonly PrinterOutputType[] OutputTypes =
    [
        PrinterOutputType.Reports,
        PrinterOutputType.Barcode,
        PrinterOutputType.Envelope,
        PrinterOutputType.Receipt
    ];

    public SavePrinterAssignmentsCommandValidator()
    {
        RuleFor(x => x.Assignments)
            .NotNull().WithMessage("تعيينات الطابعة مطلوبة.");

        RuleFor(x => x)
            .Must(HasAllFourOutputTypes).WithMessage("يجب تقديم تعيينات الطابعات الأربعة.")
            .When(x => x.Assignments != null);

        RuleForEach(x => x.Assignments)
            .Must(a => a != null && !string.IsNullOrWhiteSpace(a.PrinterName))
            .WithMessage("اسم الطابعة مطلوب.")
            .Must(a => a == null || a.PrinterName.Length <= 200)
            .WithMessage("اسم الطابعة يجب ألا يتجاوز 200 حرف.");
    }

    private static bool HasAllFourOutputTypes(SavePrinterAssignmentsCommand cmd)
    {
        if (cmd.Assignments is null)
        {
            return false;
        }

        var types = cmd.Assignments.Select(a => a.OutputType).Distinct().ToList();
        return OutputTypes.All(types.Contains) && types.Count == OutputTypes.Length;
    }
}