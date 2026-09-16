using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.Utilities.Commands.RemovePhoneBookEntry;

public sealed record RemovePhoneBookEntryCommand(int Id) : IRequest<Result>;
