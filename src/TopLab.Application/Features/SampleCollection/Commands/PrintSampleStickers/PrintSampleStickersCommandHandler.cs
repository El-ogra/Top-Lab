using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Application.Features.SampleCollection.Common;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SampleCollection.Commands.PrintSampleStickers;

public sealed class PrintSampleStickersCommandHandler : IRequestHandler<PrintSampleStickersCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly IBarcodeService _barcode;
    private readonly ISender _mediator;

    public PrintSampleStickersCommandHandler(IApplicationDbContext db, IBarcodeService barcode, ISender mediator)
    {
        _db = db;
        _barcode = barcode;
        _mediator = mediator;
    }

    public async Task<Result<int>> Handle(PrintSampleStickersCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().SingleOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Result<int>.Failure(Error.Conflict("المريض محذوف."));
        }

        var settingsResult = await _mediator.Send(new GetSystemSettingsQuery(), cancellationToken);
        var printLabId = settingsResult.IsSuccess && settingsResult.Value is not null
            ? settingsResult.Value.PrintLabIdInsteadOfPatientId
            : false;

        var drawn = _db.Set<PatientTest>()
            .Where(t => t.PatientId.Value == request.PatientId && t.IsSampleDrawn)
            .ToList();

        var scoped = drawn.Where(t => request.Scope switch
        {
            SampleStickerScope.Blood => t.IsBlood,
            SampleStickerScope.BodyFluids => t.IsUrine || t.IsStool || t.IsSemen,
            _ => true
        }).ToList();

        if (scoped.Count == 0)
        {
            return Result<int>.Failure(Error.Validation("لا توجد عينات مسحوبة لطباعة ملصقاتها."));
        }

        var labIdStr = patient.LabId?.Value;
        var printed = 0;
        foreach (var t in scoped)
        {
            var value = BarcodePayload.For(request.PatientId, labIdStr, printLabId);
            var res = await _barcode.PrintBarcodeAsync(value, cancellationToken);
            if (!res.IsSuccess)
            {
                return Result<int>.Failure(Error.Unexpected($"تمت طباعة {printed} ملصق قبل الفشل."));
            }
            printed++;
        }

        return Result<int>.Success(printed);
    }
}