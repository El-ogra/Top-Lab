using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;

namespace TopLab.Application.Features.Utilities.Queries.GetPhoneBook;

public sealed class GetPhoneBookQueryHandler
    : IRequestHandler<GetPhoneBookQuery, Result<IReadOnlyList<PhoneBookEntryDto>>>
{
    private readonly IPhoneBookStore _store;

    public GetPhoneBookQueryHandler(IPhoneBookStore store)
    {
        _store = store;
    }

    public async Task<Result<IReadOnlyList<PhoneBookEntryDto>>> Handle(
        GetPhoneBookQuery request,
        CancellationToken cancellationToken)
    {
        var load = await _store.GetAllAsync(cancellationToken);
        if (!load.IsSuccess)
        {
            return Result<IReadOnlyList<PhoneBookEntryDto>>.Failure(load.Errors);
        }

        IReadOnlyList<PhoneBookEntryDto> ordered = load.Value!
            .OrderBy(e => e.Id)
            .ToList();
        return Result<IReadOnlyList<PhoneBookEntryDto>>.Success(ordered);
    }
}
