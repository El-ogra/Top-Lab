using FluentValidation;

namespace TopLab.Application.Features.Utilities.Queries.GetTestLibrary;

public sealed class GetTestLibraryQueryValidator : AbstractValidator<GetTestLibraryQuery>
{
    public GetTestLibraryQueryValidator()
    {
        RuleFor(x => x.NameFilter)
            .MaximumLength(200)
            .When(x => x.NameFilter is not null);
    }
}
