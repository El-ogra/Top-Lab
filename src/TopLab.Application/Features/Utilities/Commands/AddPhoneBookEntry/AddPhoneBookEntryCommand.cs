using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.Utilities.Commands.AddPhoneBookEntry;

public sealed record AddPhoneBookEntryCommand(
    string Name,
    string Phone,
    string? Notes) : IRequest<Result>;
