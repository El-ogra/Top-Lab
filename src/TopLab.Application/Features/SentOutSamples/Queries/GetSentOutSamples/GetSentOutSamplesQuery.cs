using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SentOutSamples.Common;

namespace TopLab.Application.Features.SentOutSamples.Queries.GetSentOutSamples;

public sealed record GetSentOutSamplesQuery(
    DateOnly? From,
    DateOnly? To,
    int? ExternalLabEntityId,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<SentOutSampleDto>>>;
