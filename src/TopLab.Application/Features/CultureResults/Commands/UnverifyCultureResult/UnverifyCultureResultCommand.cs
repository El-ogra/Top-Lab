using MediatR; using TopLab.Application.Common.Authorization; using TopLab.Application.Common.Results; using TopLab.Application.Features.CultureResults.Common;
namespace TopLab.Application.Features.CultureResults.Commands.UnverifyCultureResult;
public sealed record UnverifyCultureResultCommand(int PatientTestId):IRequest<Result>,IAuthorizedRequest {public string RequiredPermissionCode=>CultureResultsAccessPolicy.ReviewResults;}
