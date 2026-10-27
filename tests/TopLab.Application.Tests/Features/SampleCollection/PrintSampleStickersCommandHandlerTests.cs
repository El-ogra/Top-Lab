using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SampleCollection.Commands.PrintSampleStickers;
using TopLab.Application.Features.SampleCollection.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.SampleCollection;

public class PrintSampleStickersCommandHandlerTests
{
    private sealed class RecordingBarcodeService : TopLab.Application.Common.Interfaces.IBarcodeService
    {
        public List<string> Printed { get; } = new();
        public Result? FailWith; // if set, the next print fails
        private int _callCount;

        public Task<Result> PrintBarcodeAsync(string value, CancellationToken cancellationToken = default)
        {
            _callCount++;
            if (FailWith is not null && _callCount >= 2)
            {
                var f = FailWith;
                FailWith = null;
                return Task.FromResult(f);
            }
            Printed.Add(value);
            return Task.FromResult(Result.Success());
        }
    }

    private static Patient ActivePatient(int id = 1) =>
        Patient.Create(TopLab.Domain.Common.Ids.PatientId.Create(id), "أحمد محمد", TopLab.Domain.Common.Enums.Sex.Male, 30, TopLab.Domain.Common.Enums.AgeUnit.Year, DateTime.UtcNow);

    private static PatientTest DrawnTest(int id, int patientId, bool blood = false, bool urine = false, bool stool = false, bool semen = false, bool csf = false)
    {
        var pt = PatientTest.Create(TopLab.Domain.Common.Ids.PatientTestId.Create(id), TopLab.Domain.Common.Ids.PatientId.Create(patientId), TopLab.Domain.Common.Ids.TestId.Create(1), 100m);
        pt.UpdateSampleFlags(isUrine: urine, isStool: stool, isBlood: blood, isSemen: semen, isCsf: csf, isTakenOutsideLab: false);
        pt.MarkSampleDrawn(DateTime.UtcNow);
        return pt;
    }

    [Fact]
    public async Task All_PrintsOnePerDrawnSample()
    {
        var db = new FakeApplicationDbContext();
        var patient = ActivePatient();
        db.Patients.Add(patient);
        db.PatientTests.Add(DrawnTest(1, patient.Id.Value, blood: true));
        db.PatientTests.Add(DrawnTest(2, patient.Id.Value, urine: true));

        var barcode = new RecordingBarcodeService();
        var handler = new PrintSampleStickersCommandHandler(db, barcode, new NoMediator());

        var result = await handler.Handle(new PrintSampleStickersCommand(patient.Id.Value, SampleStickerScope.All), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
        Assert.Equal(2, barcode.Printed.Count);
    }

    [Fact]
    public async Task Blood_PrintsOnlyBloodSamples()
    {
        var db = new FakeApplicationDbContext();
        var patient = ActivePatient();
        db.Patients.Add(patient);
        db.PatientTests.Add(DrawnTest(1, patient.Id.Value, blood: true));
        db.PatientTests.Add(DrawnTest(2, patient.Id.Value, urine: true));

        var barcode = new RecordingBarcodeService();
        var handler = new PrintSampleStickersCommandHandler(db, barcode, new NoMediator());

        var result = await handler.Handle(new PrintSampleStickersCommand(patient.Id.Value, SampleStickerScope.Blood), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
        Assert.Single(barcode.Printed);
    }

    [Fact]
    public async Task BodyFluids_PrintsOnlyBodyFluidSamples()
    {
        var db = new FakeApplicationDbContext();
        var patient = ActivePatient();
        db.Patients.Add(patient);
        db.PatientTests.Add(DrawnTest(1, patient.Id.Value, blood: true));
        db.PatientTests.Add(DrawnTest(2, patient.Id.Value, urine: true));
        db.PatientTests.Add(DrawnTest(3, patient.Id.Value, stool: true));

        var barcode = new RecordingBarcodeService();
        var handler = new PrintSampleStickersCommandHandler(db, barcode, new NoMediator());

        var result = await handler.Handle(new PrintSampleStickersCommand(patient.Id.Value, SampleStickerScope.BodyFluids), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
        Assert.Equal(2, barcode.Printed.Count);
    }

    [Fact]
    public async Task EmptyScope_FailsArabicMessage_NoPrints()
    {
        var db = new FakeApplicationDbContext();
        var patient = ActivePatient();
        db.Patients.Add(patient);

        var barcode = new RecordingBarcodeService();
        var handler = new PrintSampleStickersCommandHandler(db, barcode, new NoMediator());

        var result = await handler.Handle(new PrintSampleStickersCommand(patient.Id.Value, SampleStickerScope.Blood), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Validation, result.Error!.Type);
        Assert.Equal("لا توجد عينات مسحوبة لطباعة ملصقاتها.", result.Error.Message);
        Assert.Empty(barcode.Printed);
    }

    [Fact]
    public async Task PerSampleFailure_Stops_ReportsPartialCount()
    {
        var db = new FakeApplicationDbContext();
        var patient = ActivePatient();
        db.Patients.Add(patient);
        db.PatientTests.Add(DrawnTest(1, patient.Id.Value, blood: true));
        db.PatientTests.Add(DrawnTest(2, patient.Id.Value, blood: true));

        var barcode = new RecordingBarcodeService { FailWith = Result.Failure(TopLab.Application.Common.Results.Error.Unexpected("printer down")) };
        var handler = new PrintSampleStickersCommandHandler(db, barcode, new NoMediator());

        var result = await handler.Handle(new PrintSampleStickersCommand(patient.Id.Value, SampleStickerScope.All), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("1 ملصق قبل الفشل", result.Error!.Message);
        Assert.Single(barcode.Printed);
    }

    [Fact]
    public async Task Identifier_FollowsPrintLabIdInsteadOfPatientId()
    {
        var db = new FakeApplicationDbContext();
        var patient = ActivePatient();
        patient.AssignLabId(TopLab.Domain.Common.Ids.LabId.Create("LAB-1"));
        db.Patients.Add(patient);
        db.PatientTests.Add(DrawnTest(1, patient.Id.Value, blood: true));

        var barcode = new RecordingBarcodeService();
        var mediator = new SettingsMediator(printLabId: true);
        var handler = new PrintSampleStickersCommandHandler(db, barcode, mediator);

        await handler.Handle(new PrintSampleStickersCommand(patient.Id.Value, SampleStickerScope.All), CancellationToken.None);

        Assert.Single(barcode.Printed);
        Assert.Equal("LAB-1", barcode.Printed[0]);
    }

    [Fact]
    public void Requires_AddEditPatient()
    {
        var authorized = (IAuthorizedRequest)new PrintSampleStickersCommand(1, SampleStickerScope.All);
        Assert.Equal("ADD_EDIT_PATIENT", authorized.RequiredPermissionCode);
    }

    private sealed class NoMediator : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetSystemSettingsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<SystemSettingsDto>.Failure(
                    TopLab.Application.Common.Results.Error.Unexpected("settings unavailable")));
            }
            throw new NotSupportedException(request.GetType().Name);
        }
        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class SettingsMediator : ISender
    {
        private readonly bool _printLabId;
        public SettingsMediator(bool printLabId) => _printLabId = printLabId;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetSystemSettingsQuery)
            {
                return Task.FromResult((TResponse)(object)Result<SystemSettingsDto>.Success(
                    new SystemSettingsDto(TopLab.Domain.Common.Enums.AccountType.Individual, false, false, false, false, false, _printLabId, false, false,
                        TopLab.Domain.Common.Enums.ResultScreenAccountDisplayMode.Hidden, false, null, 1, false, null)));
            }
            throw new NotSupportedException(request.GetType().Name);
        }
        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
