using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestById;

public sealed record GetTestByIdQuery(int TestId) : IRequest<Result<TestDetailDto>>;