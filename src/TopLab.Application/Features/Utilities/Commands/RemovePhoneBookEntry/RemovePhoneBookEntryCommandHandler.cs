using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.Utilities.Commands.RemovePhoneBookEntry;

public sealed class RemovePhoneBookEntryCommandHandler : IRequestHandler<RemovePhoneBookEntryCommand, Result>
{
    private readonly IPhoneBookStore _store;

    public RemovePhoneBookEntryCommandHandler(IPhoneBookStore store)
    {
        _store = store;
    }

    public async Task<Result> Handle(RemovePhoneBookEntryCommand request, CancellationToken cancellationToken)
    {
        var load = await _store.GetAllAsync(cancellationToken);
        if (!load.IsSuccess)
        {
            return Result.Failure(load.Errors);
        }

        var remaining = load.Value!.Where(e => e.Id != request.Id).ToList();
        if (remaining.Count == load.Value!.Count)
        {
            return Result.Failure(Error.NotFound("البند غير موجود.", "NotFound"));
        }

        return await _store.SaveAllAsync(remaining, cancellationToken);
    }
}
