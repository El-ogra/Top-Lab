using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.SaveAnalyteReferenceRange;

public sealed record AnalyteBandInput(
    Sex? Sex,
    AgeUnit AgeUnit,
    int AgeMin,
    int AgeMax,
    decimal MinValue,
    decimal MaxValue,
    string? LowComment,
    string? HighComment);

public sealed record SaveAnalyteReferenceRangeCommand(
    int AnalyteId,
    IReadOnlyList<AnalyteBandInput> Bands)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}