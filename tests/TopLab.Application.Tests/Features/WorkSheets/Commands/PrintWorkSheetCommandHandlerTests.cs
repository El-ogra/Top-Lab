using System.Text.Json;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Commands.PrintWorkSheet;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Application.Features.WorkSheets.Queries.GetVisitWorkSheet;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.WorkSheets.Commands;

public class PrintWorkSheetCommandHandlerTests
{
    private sealed class CapturingWorkSheetPrintingService : IWorkSheetPrintingService
    {
        public string? LastToken { get; private set; }

        public Result? NextResult { get; set; }

        public Task<Result> PrintWorkSheetAsync(string workSheetToken, CancellationToken cancellationToken = default)
        {
            LastToken = workSheetToken;
            return Task.FromResult(NextResult ?? Result.Success());
        }
    }

    private static VisitWorkSheetDto Sheet()
    {
        return new VisitWorkSheetDto(
            7,
            "Ahmed",
            "100",
            new List<WorkSheetSectionDto>
            {
                new(5, "G5", new List<WorkSheetLineDto>
                {
                    new(100, 7, "Ahmed", "100", "Test T10", "T10", "BC-10", false, null, false, false, 60)
                })
            },
            new List<VisitWorkSheetSampleDto>
            {
                new(100, true, false, true, false, false, false)
            },
            1,
            false,
            false,
            false);
    }

    private static (FakeSender Sender, CapturingWorkSheetPrintingService Printing) Build(Result<VisitWorkSheetDto> sheetResult)
    {
        var sender = new FakeSender();
        sender.WithResponse(new GetVisitWorkSheetQuery(7), sheetResult);
        return (sender, new CapturingWorkSheetPrintingService());
    }

    [Fact]
    public async Task PrintWorkSheet_HappyPath_BuildsTokenFromSheetVerbatim()
    {
        var sheet = Sheet();
        var (sender, printing) = Build(Result<VisitWorkSheetDto>.Success(sheet));
        var handler = new PrintWorkSheetCommandHandler(sender, printing);

        var result = await handler.Handle(new PrintWorkSheetCommand(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(printing.LastToken);
        var envelope = JsonSerializer.Deserialize<WorkSheetPrintEnvelope>(printing.LastToken!);
        var roundTripped = JsonSerializer.Deserialize<VisitWorkSheetDto>(envelope!.WorkSheetJson);
        Assert.NotNull(roundTripped);
        Assert.Equal(sheet.PatientId, roundTripped!.PatientId);
        Assert.Equal(sheet.TotalTests, roundTripped.TotalTests);
        Assert.Single(roundTripped.Sections);
        Assert.Single(roundTripped.Samples);
    }

    [Fact]
    public async Task PrintWorkSheet_QueryFailure_PropagatesWithoutPrinting()
    {
        var (sender, printing) = Build(Result<VisitWorkSheetDto>.Failure(Error.NotFound("المريض غير موجود.")));
        var handler = new PrintWorkSheetCommandHandler(sender, printing);

        var result = await handler.Handle(new PrintWorkSheetCommand(7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Null(printing.LastToken);
    }

    [Fact]
    public async Task PrintWorkSheet_PrintingFailure_Propagates()
    {
        var (sender, printing) = Build(Result<VisitWorkSheetDto>.Success(Sheet()));
        printing.NextResult = Result.Failure(Error.Unexpected("تعذر طباعة ورقة العمل."));
        var handler = new PrintWorkSheetCommandHandler(sender, printing);

        var result = await handler.Handle(new PrintWorkSheetCommand(7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
    }

    [Fact]
    public void PrintWorkSheet_Requires_PrintWorksheet()
    {
        var authorized = (IAuthorizedRequest)new PrintWorkSheetCommand(7);

        Assert.Equal("PRINT_WORKSHEET", authorized.RequiredPermissionCode);
    }
}

public class PrintWorkSheetCommandValidatorTests
{
    private readonly PrintWorkSheetCommandValidator _validator = new();

    [Fact]
    public void Validate_ZeroPatientId_Invalid()
    {
        var result = _validator.Validate(new PrintWorkSheetCommand(0));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_PositivePatientId_Valid()
    {
        var result = _validator.Validate(new PrintWorkSheetCommand(7));

        Assert.True(result.IsValid);
    }
}
