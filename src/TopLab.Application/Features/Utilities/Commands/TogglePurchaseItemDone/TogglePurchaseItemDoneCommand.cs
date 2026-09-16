using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.Utilities.Commands.TogglePurchaseItemDone;

public sealed record TogglePurchaseItemDoneCommand(int Id) : IRequest<Result>;
