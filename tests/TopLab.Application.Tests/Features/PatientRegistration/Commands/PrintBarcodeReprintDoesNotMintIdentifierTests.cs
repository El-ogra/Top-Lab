using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.PrintBarcode;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

/// <summary>
/// P-02 S4 / SD-6 — a barcode reprint must never mint a new identifier.
///
/// The reference is explicit (REF3 p.16 §14): on a lost card, print the patient's barcode
/// again and <b>do not give a new number</b> — *"وال يعطى رقم اخر جديد"*.
///
/// This drives the REAL <see cref="PrintBarcodeCommandHandler"/> several times against a
/// capturing barcode port and asserts the patient's <c>LabId</c> is byte-identical
/// afterwards and that the same identifier is printed each time. It sits beside the
/// pre-existing <c>PrintBarcodeCommandHandlerTests</c>, which S4 leaves untouched (C-7) —
/// those cover the single-print branches; this covers the repeat-print property.
/// </summary>
public class PrintBarcodeReprintDoesNotMintIdentifierTests
{
    private sealed class CapturingBarcodeService : IBarcodeService
    {
        public List<string> Values { get; } = new();

        public Task<Result> PrintBarcodeAsync(string value, CancellationToken cancellationToken = default)
        {
            Values.Add(value);
            return Task.FromResult(Result.Success());
        }
    }

    private static SystemSettingsDto SettingsDto(bool printLabIdInsteadOfPatientId)
        => new(
            AccountType.Individual, false, false, false, false, false,
            printLabIdInsteadOfPatientId, false, false,
            ResultScreenAccountDisplayMode.Hidden, false, null);

    private static (FakeApplicationDbContext Db, FakeSender Sender, CapturingBarcodeService Barcodes) Build(
        bool printLabIdInsteadOfPatientId)
    {
        var db = new FakeApplicationDbContext();
        var sender = new FakeSender();
        sender.WithResponse(
            new GetSystemSettingsQuery(),
            Result<SystemSettingsDto>.Success(SettingsDto(printLabIdInsteadOfPatientId)));
        return (db, sender, new CapturingBarcodeService());
    }

    private static Patient PatientWithLabId(int id, string labId)
        => Patient.Create(
            PatientId.Create(id), "Ahmed Mohamed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow,
            labId: LabId.Create(labId));

    [Fact]
    public async Task ReprintingThreeTimes_LeavesLabIdUnchanged()
    {
        var (db, sender, barcodes) = Build(printLabIdInsteadOfPatientId: true);
        var patient = PatientWithLabId(7, "LAB-7");
        db.Patients.Add(patient);

        var handler = new PrintBarcodeCommandHandler(db, sender, barcodes);
        var labIdBefore = patient.LabId!.Value;

        for (var i = 0; i < 3; i++)
        {
            var result = await handler.Handle(new PrintBarcodeCommand(7), CancellationToken.None);
            Assert.True(result.IsSuccess, $"reprint {i + 1} must succeed");
        }

        // SD-6: three reprints, one unchanged identifier.
        Assert.Equal(labIdBefore, patient.LabId!.Value);
        Assert.Equal("LAB-7", patient.LabId.Value);

        // And the SAME identifier was printed each time — never a freshly issued number.
        Assert.Equal(new[] { "LAB-7", "LAB-7", "LAB-7" }, barcodes.Values.ToArray());
    }

    [Fact]
    public async Task Reprinting_PrintsThePatientId_NotANewNumber_WhenLabIdPrintingIsOff()
    {
        var (db, sender, barcodes) = Build(printLabIdInsteadOfPatientId: false);
        var patient = PatientWithLabId(9, "LAB-9");
        db.Patients.Add(patient);

        var handler = new PrintBarcodeCommandHandler(db, sender, barcodes);

        await handler.Handle(new PrintBarcodeCommand(9), CancellationToken.None);
        await handler.Handle(new PrintBarcodeCommand(9), CancellationToken.None);

        // The other branch of the existing handler: the patient's own id, every time.
        Assert.Equal(new[] { "9", "9" }, barcodes.Values.ToArray());
        Assert.Equal("LAB-9", patient.LabId!.Value);
    }

    [Fact]
    public async Task Reprinting_DoesNotChangeThePatientsIdentityFields()
    {
        var (db, sender, barcodes) = Build(printLabIdInsteadOfPatientId: true);
        var patient = PatientWithLabId(11, "LAB-11");
        db.Patients.Add(patient);

        var handler = new PrintBarcodeCommandHandler(db, sender, barcodes);
        var idBefore = patient.Id.Value;
        var labIdBefore = patient.LabId!.Value;

        await handler.Handle(new PrintBarcodeCommand(11), CancellationToken.None);

        Assert.Equal(idBefore, patient.Id.Value);
        Assert.Equal(labIdBefore, patient.LabId!.Value);
        Assert.Single(barcodes.Values);
    }

    [Fact]
    public async Task Reprinting_SoftDeletedPatient_IsRefusedWithoutPrinting()
    {
        // A reprint must not resurrect or mint anything for a deleted patient.
        var (db, sender, barcodes) = Build(printLabIdInsteadOfPatientId: true);
        var patient = PatientWithLabId(13, "LAB-13");
        patient.SoftDelete();
        db.Patients.Add(patient);

        var handler = new PrintBarcodeCommandHandler(db, sender, barcodes);
        var result = await handler.Handle(new PrintBarcodeCommand(13), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(barcodes.Values);
        Assert.Equal("LAB-13", patient.LabId!.Value);
    }
}