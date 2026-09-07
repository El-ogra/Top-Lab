using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Queries.SearchPatients;

public sealed class SearchPatientsQueryValidator : AbstractValidator<SearchPatientsQuery>
{
    public SearchPatientsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("معاملات الترقيم غير صالحة.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 500).WithMessage("معاملات الترقيم غير صالحة.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(200).WithMessage("نص البحث يجب ألا يتجاوز 200 حرفًا.");
    }
}