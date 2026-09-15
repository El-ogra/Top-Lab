using FluentValidation;

namespace TopLab.Application.Features.ReportProduction.Queries.GetMultiPatientHistory;

public sealed class GetMultiPatientHistoryQueryValidator : AbstractValidator<GetMultiPatientHistoryQuery>
{
    public GetMultiPatientHistoryQueryValidator()
    {
        RuleFor(x => x.PatientIds).NotEmpty().WithMessage("قائمة المرضى مطلوبة.");
    }
}