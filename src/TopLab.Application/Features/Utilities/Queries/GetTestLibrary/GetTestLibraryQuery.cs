using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;

namespace TopLab.Application.Features.Utilities.Queries.GetTestLibrary;

public sealed record GetTestLibraryQuery(
    string? NameFilter,
    int? TestGroupId)
    : IRequest<Result<IReadOnlyList<TestLibraryEntryDto>>>;
