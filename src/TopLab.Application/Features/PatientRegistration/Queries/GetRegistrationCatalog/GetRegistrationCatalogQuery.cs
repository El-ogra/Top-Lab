using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;

namespace TopLab.Application.Features.PatientRegistration.Queries.GetRegistrationCatalog;

public sealed record GetRegistrationCatalogQuery : IRequest<Result<RegistrationCatalogDto>>;