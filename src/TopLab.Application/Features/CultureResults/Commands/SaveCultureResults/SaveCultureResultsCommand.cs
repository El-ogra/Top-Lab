using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureResults.Common;

namespace TopLab.Application.Features.CultureResults.Commands.SaveCultureResults;

public sealed record CultureSensitivityInput(int AntibioticId, int? SensitivityCategory, decimal? InhibitionZoneMm = null);
public sealed record CultureMicroscopyInput(string? PusCells = null, string? RedBloodCells = null, string? EpithelialCells = null, string? Crystals = null, string? Fungi = null, string? OthersOne = null, string? OthersTwo = null, string? OthersThree = null, bool IsDirect = false);
public sealed record SaveCultureResultsCommand(int PatientTestId, string? Sample, string? OrganismA, string? OrganismB, string? OrganismC, string? CultureCondition, string? ColonyCount, IReadOnlyList<CultureSensitivityInput> Sensitivities, CultureMicroscopyInput? Microscopy = null) : IRequest<Result>, IAuthorizedRequest
{ public string RequiredPermissionCode => CultureResultsAccessPolicy.EditResults; }
