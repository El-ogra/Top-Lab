namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;

public sealed record PriceListSummaryDto(int Id, string Name, int ItemCount);

public sealed record PriceListItemDto(int TestId, string TestName, string TestCode, decimal Price);

public sealed record PriceListDetailDto(int Id, string Name, IReadOnlyList<PriceListItemDto> Items);
