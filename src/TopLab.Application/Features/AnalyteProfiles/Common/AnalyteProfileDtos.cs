using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.AnalyteProfiles.Common;

public sealed record AnalyteBandDto(
    Sex? Sex,
    AgeUnit AgeUnit,
    int AgeMin,
    int AgeMax,
    decimal MinValue,
    decimal MaxValue,
    string? LowComment,
    string? HighComment);

public sealed record AnalyteDefinitionDto(
    int AnalyteId,
    string Name,
    string ReportName,
    bool IsActive,
    IReadOnlyList<AnalyteBandDto> Bands);

public sealed record ProfileDefinitionDto(
    int ProfileId,
    string Name,
    int SpecializedTestId,
    string SpecializedTestName,
    decimal FixedPrice,
    bool IsActive,
    IReadOnlyList<int> AnalyteIds);