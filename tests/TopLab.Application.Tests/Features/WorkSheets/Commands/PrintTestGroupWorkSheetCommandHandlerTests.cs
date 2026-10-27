using System.Text.Json;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Commands.PrintTestGroupWorkSheet;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByTestGroup;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.WorkSheets.Commands;

/// <summary>
/// GAP-17: dedicated tests for PrintTestGroupWorkSheetCommandHandler wiring.
/// </summary>
public class PrintTestGroupWorkSheetCommandHandlerTests
{
    private sealed class CapturingPrintingService : IWorkSheetPrintingService
    {
        public string? LastToken { get; private set; }
        public Result? NextResult { get; set; }
        public Task<Result> PrintWorkSheetAsync(string workSheetToken, CancellationToken cancellationToken = default)
            => Task.FromResult(NextResult ?? Result.Success());
        public Task<Result> PrintGroupWorkSheetAsync(string workSheetToken, CancellationToken cancellationToken = default)
        {
            LastToken = workSheetToken;
            return Task.FromResult(NextResult ?? Result.Success());
        }
    }

    private static WorkSheetDto Sheet() => new(
        new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3), "TestGroup",
        new List<WorkSheetSectionDto> { new(1, "Chem", new List<WorkSheetLineDto>
        {
            new(11, 1, "Ahmed", "100", "Test T10", "T10", "BC-10", false, null, false, false, 60)
        }) },
        1, false, false, false);

    private static (FakeSender Sender, CapturingPrintingService Printing) Build(Result<WorkSheetDto> sheetResult, bool? unfinishedOnly = null)
    {
        var sender = new FakeSender();
        // FakeSender does not know GetWorkSheetByTestGroupQuery by type? It throws unless cached.
        sender.WithResponse(new GetWorkSheetByTestGroupQuery(TestGroupId: 1, From: new DateOnly(2026,9,1), To: new DateOnly(2026,9,3), UnfinishedOnly: unfinishedOnly), sheetResult);
        return (sender, new CapturingPrintingService());
    }

    [Fact]
    public async Task HappyPath_BuildsTokenFromSheet()
    {
        var sheet = Sheet();
        var (sender, printing) = Build(Result<WorkSheetDto>.Success(sheet));
        var handler = new PrintTestGroupWorkSheetCommandHandler(sender, printing);

        var result = await handler.Handle(new PrintTestGroupWorkSheetCommand(1, new DateOnly(2026,9,1), new DateOnly(2026,9,3)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(printing.LastToken);
        var envelope = JsonSerializer.Deserialize<WorkSheetGroupPrintEnvelope>(printing.LastToken!);
        Assert.NotNull(envelope);
        var roundTripped = JsonSerializer.Deserialize<WorkSheetDto>(envelope!.WorkSheetJson);
        Assert.NotNull(roundTripped);
        Assert.Equal(sheet.TotalTests, roundTripped!.TotalTests);
    }

    [Fact]
    public async Task QueryFailure_PropagatesWithoutPrinting()
    {
        var (sender, printing) = Build(Result<WorkSheetDto>.Failure(TopLab.Application.Common.Results.Error.NotFound("not found")));
        var handler = new PrintTestGroupWorkSheetCommandHandler(sender, printing);

        var result = await handler.Handle(new PrintTestGroupWorkSheetCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Null(printing.LastToken);
    }

    [Fact]
    public async Task PrintingFailure_Propagates()
    {
        var (sender, printing) = Build(Result<WorkSheetDto>.Success(Sheet()));
        printing.NextResult = Result.Failure(TopLab.Application.Common.Results.Error.Unexpected("print failed"));
        var handler = new PrintTestGroupWorkSheetCommandHandler(sender, printing);

        var result = await handler.Handle(new PrintTestGroupWorkSheetCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UnfinishedOnly_True_PropagatesToQuery()
    {
        // We cannot inspect the dispatched query directly via FakeSender; instead verify no exception and success.
        var sheet = Sheet();
        var (sender, printing) = Build(Result<WorkSheetDto>.Success(sheet), unfinishedOnly: true);
        var handler = new PrintTestGroupWorkSheetCommandHandler(sender, printing);

        var result = await handler.Handle(new PrintTestGroupWorkSheetCommand(1, UnfinishedOnly: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task EmptyResult_HandledGracefully()
    {
        var emptySheet = new WorkSheetDto(new DateOnly(2026,9,1), new DateOnly(2026,9,3), "TestGroup", new List<WorkSheetSectionDto>(), 0, false, false, false);
        var (sender, printing) = Build(Result<WorkSheetDto>.Success(emptySheet));
        var handler = new PrintTestGroupWorkSheetCommandHandler(sender, printing);

        var result = await handler.Handle(new PrintTestGroupWorkSheetCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Requires_PrintWorksheet_Permission()
    {
        var authorized = (TopLab.Application.Common.Authorization.IAuthorizedRequest)new PrintTestGroupWorkSheetCommand(1);
        Assert.Equal("PRINT_WORKSHEET", authorized.RequiredPermissionCode);
    }
}

public class PrintTestGroupWorkSheetCommandValidatorTests
{
    private readonly PrintTestGroupWorkSheetCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(new PrintTestGroupWorkSheetCommand(1, new DateOnly(2026,9,1), new DateOnly(2026,9,3)));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ZeroGroupId_IsInvalid()
    {
        var result = _validator.Validate(new PrintTestGroupWorkSheetCommand(0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_FromAfterTo_IsInvalid()
    {
        var result = _validator.Validate(new PrintTestGroupWorkSheetCommand(1, new DateOnly(2026,9,3), new DateOnly(2026,9,1)));
        Assert.False(result.IsValid);
    }
}

public class WorkSheetGroupPrintEnvelopeTests
{
    [Fact]
    public void CreateToken_SerializesSheet()
    {
        var sheet = new WorkSheetDto(new DateOnly(2026,9,1), new DateOnly(2026,9,3), "TestGroup", new List<WorkSheetSectionDto>(), 0, false, false, false);
        var token = WorkSheetGroupPrintEnvelope.CreateToken(sheet);
        Assert.False(string.IsNullOrWhiteSpace(token));
        var env = JsonSerializer.Deserialize<WorkSheetGroupPrintEnvelope>(token);
        Assert.NotNull(env);
    }

    [Fact]
    public void CreateToken_NullSheet_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => WorkSheetGroupPrintEnvelope.CreateToken(null!));
    }
}
