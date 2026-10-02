namespace TopLab.Application.Features.CultureAndAntibiotics.Common;

public sealed record AntibioticDto(
    int Id,
    string Name,
    bool IsPregnancyFlagged,
    bool IsChildrenFlagged,
    string? Symbol = null,
    string? ScientificName = null);

public sealed record AttachedAntibioticDto(
    int AntibioticId,
    string Name,
    bool IsPregnancyFlagged,
    bool IsChildrenFlagged,
    string? Symbol = null,
    string? ScientificName = null);

public sealed record CultureAntibioticListDto(
    int TestId,
    string TestName,
    int AttachedCount,
    IReadOnlyList<AttachedAntibioticDto> Antibiotics);