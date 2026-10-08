using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientEnvelope.Commands.PrintEnvelope;

public sealed class PrintEnvelopeCommandHandler : IRequestHandler<PrintEnvelopeCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ISender _sender;
    private readonly IEnvelopePrintingService _printingService;

    public PrintEnvelopeCommandHandler(
        IApplicationDbContext db,
        ISender sender,
        IEnvelopePrintingService printingService)
    {
        _db = db;
        _sender = sender;
        _printingService = printingService;
    }

    public async Task<Result> Handle(PrintEnvelopeCommand request, CancellationToken cancellationToken)
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

        var identifier = BarcodePayload.For(
            patient.Id.Value,
            patient.LabId?.Value,
            settings.Value!.PrintLabIdInsteadOfPatientId);

        // Referral line source (decision 66-D): the ExternalEntity display
        // name when a referral entity exists. When absent, the line is omitted
        // entirely — the registration placeholder source resolves to the
        // English Himself/Herself literals, which would violate the
        // no-English product rule on a printed document.
        string? referralDisplayName = null;
        if (patient.ReferralEntityId is not null)
        {
            referralDisplayName = _db.Set<ExternalEntity>()
                .FirstOrDefault(e => e.Id.Equals(patient.ReferralEntityId))?.Name;
        }

        var envelope = new EnvelopeDto(
            patient.Id.Value,
            patient.FullName,
            identifier,
            referralDisplayName,
            patient.RegistrationDateUtc,
            patient.LabId?.Value);

        var token = EnvelopePrintEnvelope.CreateToken(envelope);
        return await _printingService.PrintEnvelopeAsync(token, cancellationToken);
    }
}
