using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SentOutSamples.Common;

namespace TopLab.Application.Features.SentOutSamples.Queries.GetSentOutLabAccount;

public sealed record GetSentOutLabAccountQuery(
    int ExternalLabEntityId,
    DateOnly? From,
    DateOnly? To) : IRequest<Result<SentOutLabAccountDto>>;
