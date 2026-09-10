using FluentValidation;

namespace TopLab.Application.Features.PatientSearch.Queries.SearchPatientsGlobal;

public sealed class SearchPatientsGlobalQueryValidator : AbstractValidator<SearchPatientsGlobalQuery>
{
    public SearchPatientsGlobalQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("معاملات الترقيم غير صالحة.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 500).WithMessage("معاملات الترقيم غير صالحة.");

        RuleFor(x => x.Text)
            .MaximumLength(200).WithMessage("نص البحث يجب ألا يتجاوز 200 حرفًا.");
    }
}