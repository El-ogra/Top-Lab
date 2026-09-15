using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.ReportProduction.Commands.BuildBlankReport;

public sealed class BuildBlankReportCommandHandler
    : IRequestHandler<BuildBlankReportCommand, Result<BlankReportDto>>
{
    private readonly IApplicationDbContext _db;

    public BuildBlankReportCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<BlankReportDto>> Handle(
        BuildBlankReportCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<BlankReportDto>.Failure(
                Error.NotFound("المريض غير موجود.")));
        }

        var externalIds = new[] { patient.TreatingDoctorId?.Value, patient.ReferralEntityId?.Value }
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        var entities = externalIds.Count == 0
            ? new Dictionary<int, ExternalEntity>()
            : _db.Set<ExternalEntity>()
                .Where(e => externalIds.Contains(e.Id.Value))
                .ToDictionary(e => e.Id.Value);

        string? doctor = patient.TreatingDoctorId is not null
            && entities.TryGetValue(patient.TreatingDoctorId.Value, out var doctorEntity)
            ? doctorEntity.Name
            : null;

        string? referral = patient.ReferralEntityId is not null
            && entities.TryGetValue(patient.ReferralEntityId.Value, out var referralEntity)
            ? referralEntity.Name
            : null;

        var dto = new BlankReportDto(
            patient.Id.Value,
            patient.FullName,
            patient.LabId?.Value,
            patient.Sex.ToString(),
            patient.AgeValue,
            patient.AgeUnit.ToString(),
            doctor,
            referral);

        return Task.FromResult(Result<BlankReportDto>.Success(dto));
    }
}