using FluentValidation;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTest;

public sealed class CreateTestCommandValidator : AbstractValidator<CreateTestCommand>
{
    public CreateTestCommandValidator()
    {
        RuleFor(x => x.TestCode)
            .NotEmpty().WithMessage("كود التحليل مطلوب.")
            .MaximumLength(50).WithMessage("كود التحليل يجب ألا يتجاوز 50 حرفًا.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("الاسم مطلوب.")
            .MaximumLength(150).WithMessage("الاسم يجب ألا يتجاوز 150 حرفًا.");

        RuleFor(x => x.ReportName)
            .NotEmpty().WithMessage("اسم التقرير مطلوب.")
            .MaximumLength(150).WithMessage("اسم التقرير يجب ألا يتجاوز 150 حرفًا.");

        RuleFor(x => x.ReceiptName)
            .NotEmpty().WithMessage("اسم الإيصال مطلوب.")
            .MaximumLength(150).WithMessage("اسم الإيصال يجب ألا يتجاوز 150 حرفًا.");

        RuleFor(x => x.Barcode)
            .MaximumLength(50).WithMessage("الباركود يجب ألا يتجاوز 50 حرفًا.");

        RuleFor(x => x.CompletionDurationMinutes)
            .GreaterThan(0).WithMessage("مدة الإنجاز يجب أن تكون أكبر من صفر.");

        RuleFor(x => x.PatientPrice)
            .GreaterThanOrEqualTo(0).WithMessage("سعر المريض يجب ألا يكون سالبًا.");

        When(x => x.LabToLabPrice.HasValue, () =>
        {
            RuleFor(x => x.LabToLabPrice)
                .GreaterThanOrEqualTo(0).WithMessage("سعر المعمل للآخر يجب ألا يكون سالبًا.");
        });

        RuleFor(x => x)
            .Must(x => !x.IsSentOut || x.SentOutCostPrice.HasValue)
            .WithMessage("سعر الإرسال مطلوب عندما يكون التحليل صادرًا.");

        RuleFor(x => x.ResultKind)
            .IsInEnum().WithMessage("نوع النتيجة غير صالح.");
    }
}