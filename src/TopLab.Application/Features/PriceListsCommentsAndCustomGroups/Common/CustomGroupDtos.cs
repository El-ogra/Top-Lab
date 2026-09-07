namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;

public sealed record CustomGroupSummaryDto(int Id, string Name, int ItemCount);

public sealed record CustomGroupItemDto(int TestId, string TestName, string TestCode, decimal Price);

public sealed record CustomGroupDetailDto(int Id, string Name, IReadOnlyList<CustomGroupItemDto> Items);
