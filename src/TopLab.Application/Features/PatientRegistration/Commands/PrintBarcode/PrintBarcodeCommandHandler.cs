using System.Globalization;
using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientRegistration.Commands.PrintBarcode;

public sealed class PrintBarcodeCommandHandler : IRequestHandler<PrintBarcodeCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ISender _sender;
    private readonly IBarcodeService _barcodeService;

    public PrintBarcodeCommandHandler(
        IApplicationDbContext db,
        ISender sender,
        IBarcodeService barcodeService)
    {
        _db = db;
        _sender = sender;
        _barcodeService = barcodeService;
    }

    public async Task<Result> Handle(PrintBarcodeCommand request, CancellationToken cancellationToken)
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

        var identifier = settings.Value!.PrintLabIdInsteadOfPatientId && patient.LabId is not null
            ? patient.LabId.Value
            : patient.Id.Value.ToString(CultureInfo.InvariantCulture);

        return await _barcodeService.PrintBarcodeAsync(identifier, cancellationToken);
    }
}
