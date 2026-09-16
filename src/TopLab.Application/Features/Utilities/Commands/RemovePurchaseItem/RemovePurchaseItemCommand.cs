using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.Utilities.Commands.RemovePurchaseItem;

public sealed record RemovePurchaseItemCommand(int Id) : IRequest<Result>;
