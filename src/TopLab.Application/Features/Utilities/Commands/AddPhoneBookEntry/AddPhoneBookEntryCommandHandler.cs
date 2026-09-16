using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;

namespace TopLab.Application.Features.Utilities.Commands.AddPhoneBookEntry;

public sealed class AddPhoneBookEntryCommandHandler : IRequestHandler<AddPhoneBookEntryCommand, Result>
{
    private readonly IPhoneBookStore _store;

    public AddPhoneBookEntryCommandHandler(IPhoneBookStore store)
    {
        _store = store;
    }

    public async Task<Result> Handle(AddPhoneBookEntryCommand request, CancellationToken cancellationToken)
    {
        var load = await _store.GetAllAsync(cancellationToken);
        if (!load.IsSuccess)
        {
            return Result.Failure(load.Errors);
        }

        var entries = load.Value!.ToList();
        var nextId = entries.Count == 0 ? 1 : entries.Max(e => e.Id) + 1;
        entries.Add(new PhoneBookEntryDto(nextId, request.Name.Trim(), request.Phone.Trim(), request.Notes?.Trim()));

        return await _store.SaveAllAsync(entries, cancellationToken);
    }
}
