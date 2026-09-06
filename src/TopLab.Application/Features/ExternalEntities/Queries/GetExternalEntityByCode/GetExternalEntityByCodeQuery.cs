using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Common;

namespace TopLab.Application.Features.ExternalEntities.Queries.GetExternalEntityByCode;

public sealed record GetExternalEntityByCodeQuery(string GeneratedIdCode) : IRequest<Result<ExternalEntityDetailDto>>;
