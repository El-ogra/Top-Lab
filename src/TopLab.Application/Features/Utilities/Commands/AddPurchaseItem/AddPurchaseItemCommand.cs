using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.Utilities.Commands.AddPurchaseItem;

public sealed record AddPurchaseItemCommand(string Text) : IRequest<Result>;
