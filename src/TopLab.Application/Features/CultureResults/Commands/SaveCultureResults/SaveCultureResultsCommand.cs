using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureResults.Common;

namespace TopLab.Application.Features.CultureResults.Commands.SaveCultureResults;

public sealed record CultureSensitivityInput(int AntibioticId, int SensitivityCategory);
public sealed record SaveCultureResultsCommand(int PatientTestId, string? Sample, string? OrganismA, string? OrganismB, string? OrganismC, string? CultureCondition, string? ColonyCount, IReadOnlyList<CultureSensitivityInput> Sensitivities) : IRequest<Result>, IAuthorizedRequest
{ public string RequiredPermissionCode => CultureResultsAccessPolicy.EditResults; }
