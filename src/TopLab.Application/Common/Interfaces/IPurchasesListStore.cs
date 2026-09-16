using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Port for the workstation-local Requirements &amp; Purchases list
/// (JSON file under the workstation configuration directory — SD-23-2).
/// No database table, no EF entity, no migration.
/// </summary>
public interface IPurchasesListStore
{
    Task<Result<IReadOnlyList<PurchaseItemDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result> SaveAllAsync(IReadOnlyList<PurchaseItemDto> items, CancellationToken cancellationToken = default);
}
