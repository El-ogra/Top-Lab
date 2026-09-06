using FluentValidation;

namespace TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;

public sealed class SearchExternalEntitiesQueryValidator : AbstractValidator<SearchExternalEntitiesQuery>
{
    public SearchExternalEntitiesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("معاملات الترقيم غير صالحة.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("معاملات الترقيم غير صالحة.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(200).WithMessage("نص البحث يجب ألا يتجاوز 200 حرفًا.");

        When(x => x.EntityType.HasValue, () =>
        {
            RuleFor(x => x.EntityType!)
                .IsInEnum().WithMessage("نوع الجهة غير صالح.");
        });
    }
}
