using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Common;

namespace TopLab.Application.Features.ExternalEntities.Queries.GetExternalEntityById;

public sealed record GetExternalEntityByIdQuery(int Id) : IRequest<Result<ExternalEntityDetailDto>>;
