using System.Text.Json;
using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Commands.PrintGroupPatientSlips;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.ResultDelivery.Queries.GetGroupPatientSlips;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultDelivery;

/// <summary>
/// Phase 6 (GAP-013): the print command resolves the slips through the query and
/// hands the batch to the printing service as a single token.
/// </summary>
public class PrintGroupPatientSlipsCommandHandlerTests
{
    private sealed class CapturingGroupSlipPrintingService : IGroupPatientSlipPrintingService
    {
        public string? LastToken { get; private set; }

        public Result? NextResult { get; set; }

        public Task<Result> PrintGroupPatientSlipsAsync(string batchToken, CancellationToken cancellationToken = default)
        {
            LastToken = batchToken;
            return Task.FromResult(NextResult ?? Result.Success());
        }
    }

    private sealed class CapturingSender : ISender
    {
        public GetGroupPatientSlipsQuery? LastQuery { get; private set; }

        public Result<GroupPatientSlipBatchDto>? NextResult { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetGroupPatientSlipsQuery query)
            {
                LastQuery = query;
                var response = NextResult ?? Result<GroupPatientSlipBatchDto>.Success(
                    new GroupPatientSlipBatchDto(new List<GroupPatientSlipDto>()));
                return Task.FromResult((TResponse)(object)response);
            }

            throw new NotSupportedException($"CapturingSender has no canned response for {request.GetType().Name}.");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private static GroupPatientSlipDto Slip(int patientId)
    {
        return new GroupPatientSlipDto(
            patientId,
            $"Patient {patientId}",
            "LAB-A",
            "LAB-A",
            new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc),
            new List<GroupPatientSlipLineDto> { new("CBC", "صورة دم كاملة") });
    }

    [Fact]
    public async Task Print_ResolvesSlipsThroughQuery_AndPassesTheBatchToTheWriter()
    {
        var sender = new CapturingSender
        {
            NextResult = Result<GroupPatientSlipBatchDto>.Success(
                new GroupPatientSlipBatchDto(new List<GroupPatientSlipDto> { Slip(1), Slip(2) }))
        };
        var printing = new CapturingGroupSlipPrintingService();
        var handler = new PrintGroupPatientSlipsCommandHandler(sender, printing);

        var result = await handler.Handle(new PrintGroupPatientSlipsCommand(new[] { 1, 2 }), CancellationToken.None);

        Assert.True(result.IsSuccess);

        // Selection-to-query parameters.
        Assert.NotNull(sender.LastQuery);
        Assert.Equal(new[] { 1, 2 }, sender.LastQuery!.PatientIds.ToArray());

        // Writer invocation: one token carrying both slips.
        Assert.NotNull(printing.LastToken);
        var envelope = JsonSerializer.Deserialize<GroupPatientSlipPrintEnvelope>(printing.LastToken!);
        Assert.NotNull(envelope);
        var batch = JsonSerializer.Deserialize<GroupPatientSlipBatchDto>(envelope!.BatchJson);
        Assert.NotNull(batch);
        Assert.Equal(2, batch!.Slips.Count);
        Assert.Equal("CBC", batch.Slips[0].Lines[0].TestCode);
    }

    [Fact]
    public async Task Print_EmptyBatch_StillInvokesTheWriterSafely()
    {
        var sender = new CapturingSender();
        var printing = new CapturingGroupSlipPrintingService();
        var handler = new PrintGroupPatientSlipsCommandHandler(sender, printing);

        var result = await handler.Handle(new PrintGroupPatientSlipsCommand(new[] { 1 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(printing.LastToken);
        var envelope = JsonSerializer.Deserialize<GroupPatientSlipPrintEnvelope>(printing.LastToken!);
        var batch = JsonSerializer.Deserialize<GroupPatientSlipBatchDto>(envelope!.BatchJson);
        Assert.Empty(batch!.Slips);
    }

    [Fact]
    public async Task Print_QueryFailure_PropagatesWithoutPrinting()
    {
        var sender = new CapturingSender
        {
            NextResult = Result<GroupPatientSlipBatchDto>.Failure(Error.NotFound("المريض غير موجود."))
        };
        var printing = new CapturingGroupSlipPrintingService();
        var handler = new PrintGroupPatientSlipsCommandHandler(sender, printing);

        var result = await handler.Handle(new PrintGroupPatientSlipsCommand(new[] { 99 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Null(printing.LastToken);
    }

    [Fact]
    public async Task Print_PrintingServiceFailure_Propagates()
    {
        var sender = new CapturingSender();
        var printing = new CapturingGroupSlipPrintingService
        {
            NextResult = Result.Failure(Error.Unexpected("تعذر طباعة قصاصات المرضى."))
        };
        var handler = new PrintGroupPatientSlipsCommandHandler(sender, printing);

        var result = await handler.Handle(new PrintGroupPatientSlipsCommand(new[] { 1 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
    }

    [Fact]
    public void Print_Requires_DeliverResults()
    {
        IAuthorizedRequest authorized = new PrintGroupPatientSlipsCommand(new[] { 1 });

        Assert.Equal("DELIVER_RESULTS", authorized.RequiredPermissionCode);
        Assert.Equal(ResultDeliveryAccessPolicy.DeliverResults, authorized.RequiredPermissionCode);
    }

    [Fact]
    public void Query_Requires_DeliverResults()
    {
        IAuthorizedRequest authorized = new GetGroupPatientSlipsQuery(new[] { 1 });

        Assert.Equal("DELIVER_RESULTS", authorized.RequiredPermissionCode);
    }
}

public class PrintGroupPatientSlipsCommandValidatorTests
{
    private readonly PrintGroupPatientSlipsCommandValidator _validator = new();

    [Fact]
    public void Validate_EmptySelection_Invalid_WithFrozenMessage()
    {
        var result = _validator.Validate(new PrintGroupPatientSlipsCommand(Array.Empty<int>()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "قائمة المرضى مطلوبة.");
    }

    [Fact]
    public void Validate_NonPositivePatientId_Invalid()
    {
        var result = _validator.Validate(new PrintGroupPatientSlipsCommand(new[] { -1 }));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "معرّف المريض غير صالح.");
    }

    [Fact]
    public void Validate_ValidIds_Valid()
    {
        var result = _validator.Validate(new PrintGroupPatientSlipsCommand(new[] { 1, 2 }));

        Assert.True(result.IsValid);
    }
}
