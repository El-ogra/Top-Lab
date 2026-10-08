using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientEnvelope.Commands.PrintLabOrder;

public sealed class PrintLabOrderCommandHandler : IRequestHandler<PrintLabOrderCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ISender _sender;
    private readonly ILabOrderPrintingService _printingService;

    public PrintLabOrderCommandHandler(
        IApplicationDbContext db,
        ISender sender,
        ILabOrderPrintingService printingService)
    {
        _db = db;
        _sender = sender;
        _printingService = printingService;
    }

    public async Task<Result> Handle(PrintLabOrderCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Result.Failure(Error.NotFound("المريض غير موجود."));
        }

        var settings = await _sender.Send(new GetSystemSettingsQuery(), cancellationToken);
        if (!settings.IsSuccess)
        {
            return Result.Failure(settings.Error!);
        }

        // Line source verification (handoff D.2 item 2): PatientBillingReader
        // reads the visit's PatientTest rows directly (not payment-gated), so
        // the slip lists the ordered tests including uninvoiced/unpaid ones.
        var account = PatientBillingReader.ReadAccount(_db, patient);

        var identifier = BarcodePayload.For(
            patient.Id.Value,
            patient.LabId?.Value,
            settings.Value!.PrintLabIdInsteadOfPatientId);

        var order = new LabOrderDto(
            patient.Id.Value,
            patient.FullName,
            patient.LabId?.Value,
            identifier,
            patient.RegistrationDateUtc,
            account.ChargedTests
                .Select(t => new LabOrderLineDto(t.TestCode, t.TestName))
                .ToList());

        var token = LabOrderPrintEnvelope.CreateToken(order);
        return await _printingService.PrintLabOrderAsync(token, cancellationToken);
    }
}
