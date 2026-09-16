using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;

namespace TopLab.Application.Features.Utilities.Queries.GetPhoneBook;

public sealed record GetPhoneBookQuery : IRequest<Result<IReadOnlyList<PhoneBookEntryDto>>>;
