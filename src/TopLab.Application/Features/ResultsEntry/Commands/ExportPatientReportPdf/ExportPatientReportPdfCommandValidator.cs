using FluentValidation;

namespace TopLab.Application.Features.ResultsEntry.Commands.ExportPatientReportPdf;

public sealed class ExportPatientReportPdfCommandValidator : AbstractValidator<ExportPatientReportPdfCommand>
{
    public ExportPatientReportPdfCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");

        RuleFor(x => x.AbsolutePath)
            .NotEmpty().WithMessage("مسار ملف PDF مطلوب.")
            .Must(p => string.IsNullOrWhiteSpace(p) || System.IO.Path.IsPathFullyQualified(p))
            .WithMessage("مسار ملف PDF يجب أن يكون مطلقًا.")
            .Must(p => string.IsNullOrWhiteSpace(p) || p.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            .WithMessage("مسار الملف يجب أن يكون بامتداد PDF.");
    }
}
