using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Features.Attendance.Commands.CheckIn;
using TopLab.Application.Features.Attendance.Commands.CheckOut;
using TopLab.Application.Features.Attendance.Commands.EndBreak;
using TopLab.Application.Features.Attendance.Commands.StartBreak;
using TopLab.Application.Features.Attendance.Queries.GetAttendanceRecords;
using TopLab.Application.Features.Attendance.Queries.GetUserAttendanceSummary;
using TopLab.Application.Features.AuditAndTraceability.Queries.GetPatientAudit;
using TopLab.Application.Features.AuditAndTraceability.Queries.GetPatientTestAudit;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.AttachAntibioticToCulture;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.CreateAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.DeleteAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.DetachAntibioticFromCulture;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.UpdateAntibiotic;
using TopLab.Application.Features.ExternalEntities.Commands.CreateExternalEntity;
using TopLab.Application.Features.ExternalEntities.Commands.DeleteExternalEntity;
using TopLab.Application.Features.ExternalEntities.Commands.GenerateEntityIdCode;
using TopLab.Application.Features.ExternalEntities.Commands.UpdateExternalEntity;
using TopLab.Application.Features.PatientRegistration.Commands.AddCustomGroupToVisit;
using TopLab.Application.Features.PatientRegistration.Commands.AddMedicalCondition;
using TopLab.Application.Features.PatientRegistration.Commands.AddTestsToVisit;
using TopLab.Application.Features.PatientRegistration.Commands.ClearAllTests;
using TopLab.Application.Features.PatientRegistration.Commands.CreatePatient;
using TopLab.Application.Features.PatientRegistration.Commands.RemoveTestFromVisit;
using TopLab.Application.Features.PatientRegistration.Commands.UpdatePatientTestSampleFlags;
using TopLab.Application.Features.ReportProduction.Commands.PrintBlankReport;
using TopLab.Application.Features.ReportProduction.Commands.PrintCombinedReport;
using TopLab.Application.Features.ReportProduction.Commands.PrintHistoryReport;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreateCustomGroup;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreatePriceList;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreateTestComment;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeleteCustomGroup;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeletePriceList;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeleteTestComment;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RemoveCustomGroupItem;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RemovePriceListItem;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RenameCustomGroup;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RenamePriceList;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.SetCustomGroupItemPrice;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.SetPriceListItemPrice;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.UpdateTestComment;
using TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;
using TopLab.Application.Features.ResultsEntry.Commands.ClearResult;
using TopLab.Application.Features.ResultsEntry.Commands.EnterResult;
using TopLab.Application.Features.ResultsEntry.Commands.ExportPatientReportPdf;
using TopLab.Application.Features.ResultsEntry.Commands.MarkAllPatientResultsReviewed;
using TopLab.Application.Features.ResultsEntry.Commands.MarkResultDelivered;
using TopLab.Application.Features.ResultsEntry.Commands.MarkResultPrinted;
using TopLab.Application.Features.ResultsEntry.Commands.RefreshResultReferenceRange;
using TopLab.Application.Features.ResultsEntry.Commands.ReviewResult;
using TopLab.Application.Features.ResultsEntry.Commands.UnreviewResult;
using TopLab.Application.Features.ResultsEntry.Queries.GetPatientResultSheet;
using TopLab.Application.Features.ResultsEntry.Queries.GetResultEntry;
using TopLab.Application.Features.ResultsEntry.Queries.GetResultWorklist;
using TopLab.Application.Features.ResultDelivery.Commands.DeliverWithSettlement;
using TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryGrid;
using TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;
using TopLab.Application.Features.SentOutSamples.Commands.RecordSentOutPayment;
using TopLab.Application.Features.SentOutSamples.Commands.SendSampleOut;
using TopLab.Application.Features.SentOutSamples.Commands.SettleSentOutInFull;
using TopLab.Application.Features.SentOutSamples.Queries.GetSentOutLabAccount;
using TopLab.Application.Features.SentOutSamples.Queries.GetSentOutSamples;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTest;

namespace TopLab.Application.Tests.DependencyInjection;

public class ValidatorRegistrationTests
{
    [Fact]
    public void HostBuiltLikeApp_ResolvesCreateTestValidator()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetService<IValidator<CreateTestCommand>>();

        Assert.NotNull(validator);
    }

    [Fact]
    public void HostBuiltLikeApp_RegisteredValidator_ActuallyValidates()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetRequiredService<IValidator<CreateTestCommand>>();
        var command = new CreateTestCommand("", "", "", "", 0, -5m, default, false, null, null, false, null, null);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(typeof(IValidator<CreateExternalEntityCommand>))]
    [InlineData(typeof(IValidator<UpdateExternalEntityCommand>))]
    [InlineData(typeof(IValidator<DeleteExternalEntityCommand>))]
    [InlineData(typeof(IValidator<GenerateEntityIdCodeCommand>))]
    public void HostBuiltLikeApp_ResolvesExternalEntityValidators(System.Type validatorType)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetService(validatorType);

        Assert.NotNull(validator);
    }

    [Theory]
    [InlineData(typeof(IValidator<PrintCombinedReportCommand>))]
    [InlineData(typeof(IValidator<PrintBlankReportCommand>))]
    [InlineData(typeof(IValidator<PrintHistoryReportCommand>))]
    public void HostBuiltLikeApp_ResolvesM07PrintValidators(System.Type validatorType)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetService(validatorType);

        Assert.NotNull(validator);
    }

    [Theory]
    [InlineData(typeof(IValidator<CreatePriceListCommand>))]
    [InlineData(typeof(IValidator<RenamePriceListCommand>))]
    [InlineData(typeof(IValidator<DeletePriceListCommand>))]
    [InlineData(typeof(IValidator<SetPriceListItemPriceCommand>))]
    [InlineData(typeof(IValidator<RemovePriceListItemCommand>))]
    [InlineData(typeof(IValidator<CreateTestCommentCommand>))]
    [InlineData(typeof(IValidator<UpdateTestCommentCommand>))]
    [InlineData(typeof(IValidator<DeleteTestCommentCommand>))]
    [InlineData(typeof(IValidator<CreateCustomGroupCommand>))]
    [InlineData(typeof(IValidator<RenameCustomGroupCommand>))]
    [InlineData(typeof(IValidator<DeleteCustomGroupCommand>))]
    [InlineData(typeof(IValidator<SetCustomGroupItemPriceCommand>))]
    [InlineData(typeof(IValidator<RemoveCustomGroupItemCommand>))]
    public void HostBuiltLikeApp_ResolvesM13Validators(System.Type validatorType)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetService(validatorType);

        Assert.NotNull(validator);
    }

    [Theory]
    [InlineData(typeof(IValidator<CreateAntibioticCommand>))]
    [InlineData(typeof(IValidator<UpdateAntibioticCommand>))]
    [InlineData(typeof(IValidator<DeleteAntibioticCommand>))]
    [InlineData(typeof(IValidator<AttachAntibioticToCultureCommand>))]
    [InlineData(typeof(IValidator<DetachAntibioticFromCultureCommand>))]
    public void HostBuiltLikeApp_ResolvesM15Validators(System.Type validatorType)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetService(validatorType);

        Assert.NotNull(validator);
    }

    [Theory]
    [InlineData(typeof(IValidator<CreatePatientCommand>))]
    [InlineData(typeof(IValidator<AddMedicalConditionCommand>))]
    [InlineData(typeof(IValidator<AddTestsToVisitCommand>))]
    [InlineData(typeof(IValidator<AddCustomGroupToVisitCommand>))]
    [InlineData(typeof(IValidator<UpdatePatientTestSampleFlagsCommand>))]
    [InlineData(typeof(IValidator<RemoveTestFromVisitCommand>))]
    public void HostBuiltLikeApp_ResolvesM02Validators(System.Type validatorType)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetService(validatorType);

        Assert.NotNull(validator);
    }

    [Theory]
    [InlineData(typeof(IValidator<GetResultWorklistQuery>))]
    [InlineData(typeof(IValidator<GetResultEntryQuery>))]
    [InlineData(typeof(IValidator<GetPatientResultSheetQuery>))]
    [InlineData(typeof(IValidator<EnterResultCommand>))]
    [InlineData(typeof(IValidator<ClearResultCommand>))]
    [InlineData(typeof(IValidator<RefreshResultReferenceRangeCommand>))]
    [InlineData(typeof(IValidator<ReviewResultCommand>))]
    [InlineData(typeof(IValidator<UnreviewResultCommand>))]
    [InlineData(typeof(IValidator<MarkResultPrintedCommand>))]
    [InlineData(typeof(IValidator<MarkResultDeliveredCommand>))]
    [InlineData(typeof(IValidator<MarkAllPatientResultsReviewedCommand>))]
    [InlineData(typeof(IValidator<BulkPrintPreflightQuery>))]
    [InlineData(typeof(IValidator<ExecuteBulkPrintCommand>))]
    [InlineData(typeof(IValidator<ExportPatientReportPdfCommand>))]
    public void HostBuiltLikeApp_ResolvesM04Validators(System.Type validatorType)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetService(validatorType);

        Assert.NotNull(validator);
    }

    [Theory]
    [InlineData(typeof(IValidator<GetUndeliveredResultsQuery>))]
    [InlineData(typeof(IValidator<GetDeliveryGridQuery>))]
    [InlineData(typeof(IValidator<DeliverWithSettlementCommand>))]
    public void HostBuiltLikeApp_ResolvesM09Validators(System.Type validatorType)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetService(validatorType);

        Assert.NotNull(validator);
    }

    [Theory]
    [InlineData(typeof(IValidator<SendSampleOutCommand>))]
    [InlineData(typeof(IValidator<RecordSentOutPaymentCommand>))]
    [InlineData(typeof(IValidator<SettleSentOutInFullCommand>))]
    [InlineData(typeof(IValidator<GetSentOutSamplesQuery>))]
    [InlineData(typeof(IValidator<GetSentOutLabAccountQuery>))]
    public void HostBuiltLikeApp_ResolvesM16Validators(System.Type validatorType)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetService(validatorType);

        Assert.NotNull(validator);
    }

    [Theory]
    [InlineData(typeof(IValidator<GetPatientAuditQuery>))]
    [InlineData(typeof(IValidator<GetPatientTestAuditQuery>))]
    public void HostBuiltLikeApp_ResolvesM10Validators(System.Type validatorType)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetService(validatorType);

        Assert.NotNull(validator);
    }

    [Theory]
    [InlineData(typeof(IValidator<CheckInCommand>))]
    [InlineData(typeof(IValidator<StartBreakCommand>))]
    [InlineData(typeof(IValidator<EndBreakCommand>))]
    [InlineData(typeof(IValidator<CheckOutCommand>))]
    [InlineData(typeof(IValidator<GetAttendanceRecordsQuery>))]
    [InlineData(typeof(IValidator<GetUserAttendanceSummaryQuery>))]
    public void HostBuiltLikeApp_ResolvesM18Validators(System.Type validatorType)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var validator = provider.GetService(validatorType);

        Assert.NotNull(validator);
    }
}