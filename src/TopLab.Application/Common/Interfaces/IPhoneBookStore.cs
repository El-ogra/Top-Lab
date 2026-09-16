using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Port for the workstation-local Tools Phone Book (SD-23-2/SD-23-11).
/// Fully separate from patient phone numbers — no Patient linkage of any kind.
/// </summary>
public interface IPhoneBookStore
{
    Task<Result<IReadOnlyList<PhoneBookEntryDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result> SaveAllAsync(IReadOnlyList<PhoneBookEntryDto> entries, CancellationToken cancellationToken = default);
}
