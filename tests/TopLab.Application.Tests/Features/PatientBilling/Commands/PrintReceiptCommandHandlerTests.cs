using System.Text.Json;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.PrintReceipt;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientReceipt;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientBilling.Commands;

public class PrintReceiptCommandHandlerTests
{
    private sealed class CapturingReceiptPrintingService : IReceiptPrintingService
    {
        public string? LastToken { get; private set; }

        public Result? NextResult { get; set; }

        public Task<Result> PrintReceiptAsync(string receiptToken, CancellationToken cancellationToken = default)
        {
            LastToken = receiptToken;
            return Task.FromResult(NextResult ?? Result.Success());
        }
    }

    private static ReceiptDto Dto()
    {
        return new ReceiptDto(
            7,
            "أحمد محمد علي",
            "100",
            new List<ChargedTestDto>
            {
                new(1, "صورة دم كاملة", "CBC", "CBC", 100m)
            },
            100m,
            10m,
            40m,
            50m,
            "L.E.");
    }

    private static (FakeSender Sender, CapturingReceiptPrintingService Printing) Build(Result<ReceiptDto> receiptResult)
    {
        var sender = new FakeSender();
        sender.WithResponse(new GetPatientReceiptQuery(7), receiptResult);
        return (sender, new CapturingReceiptPrintingService());
    }

    [Fact]
    public async Task PrintReceipt_HappyPath_BuildsTokenFromReceiptDtoVerbatim()
    {
        var dto = Dto();
        var (sender, printing) = Build(Result<ReceiptDto>.Success(dto));
        var handler = new PrintReceiptCommandHandler(sender, printing);

        var result = await handler.Handle(new PrintReceiptCommand(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(printing.LastToken);
        var envelope = JsonSerializer.Deserialize<ReceiptPrintEnvelope>(printing.LastToken!);
        Assert.NotNull(envelope);
        var roundTripped = JsonSerializer.Deserialize<ReceiptDto>(envelope!.ReceiptJson);
        Assert.NotNull(roundTripped);
        Assert.Equal(dto.PatientId, roundTripped!.PatientId);
        Assert.Equal(dto.PatientFullName, roundTripped.PatientFullName);
        Assert.Equal(dto.LabId, roundTripped.LabId);
        Assert.Equal(dto.TotalCharged, roundTripped.TotalCharged);
        Assert.Equal(dto.TotalDiscount, roundTripped.TotalDiscount);
        Assert.Equal(dto.TotalPaid, roundTripped.TotalPaid);
        Assert.Equal(dto.Balance, roundTripped.Balance);
        Assert.Equal(dto.Currency, roundTripped.Currency);
        Assert.Single(roundTripped.ChargedTests);
    }

    [Fact]
    public async Task PrintReceipt_PatientNotFound_PropagatesWithoutPrinting()
    {
        var (sender, printing) = Build(Result<ReceiptDto>.Failure(Error.NotFound("المريض غير موجود.")));
        var handler = new PrintReceiptCommandHandler(sender, printing);

        var result = await handler.Handle(new PrintReceiptCommand(7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error.Message);
        Assert.Null(printing.LastToken);
    }

    [Fact]
    public async Task PrintReceipt_PrintingServiceFailure_Propagates()
    {
        var (sender, printing) = Build(Result<ReceiptDto>.Success(Dto()));
        printing.NextResult = Result.Failure(Error.Unexpected("تعذر طباعة الإيصال."));
        var handler = new PrintReceiptCommandHandler(sender, printing);

        var result = await handler.Handle(new PrintReceiptCommand(7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
    }
}

public class PrintReceiptCommandValidatorTests
{
    private readonly PrintReceiptCommandValidator _validator = new();

    [Fact]
    public void Validate_ZeroPatientId_Invalid()
    {
        var result = _validator.Validate(new PrintReceiptCommand(0));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_PositivePatientId_Valid()
    {
        var result = _validator.Validate(new PrintReceiptCommand(7));

        Assert.True(result.IsValid);
    }
}
