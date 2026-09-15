using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SentOutSamples.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.SentOutSamples;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.SentOutSamples.Commands.SendSampleOut;

public sealed class SendSampleOutCommandHandler : IRequestHandler<SendSampleOutCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _dateTime;

    public SendSampleOutCommandHandler(IApplicationDbContext db, IDateTimeProvider dateTime)
    {
        _db = db;
        _dateTime = dateTime;
    }

    public async Task<Result<int>> Handle(SendSampleOutCommand request, CancellationToken cancellationToken)
    {
        var patientTest = _db.Set<PatientTest>().FirstOrDefault(pt => pt.Id.Value == request.PatientTestId);
        if (patientTest is null)
        {
            return Result<int>.Failure(Error.NotFound("التحليل غير موجود"));
        }

        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == patientTest.PatientId.Value);
        if (patient is null || patient.IsDeleted)
        {
            return Result<int>.Failure(Error.NotFound("المريض غير موجود."));
        }

        var test = _db.Set<Test>().FirstOrDefault(t => t.Id.Value == patientTest.TestId.Value);
        if (test is null)
        {
            return Result<int>.Failure(Error.NotFound("التحليل غير موجود"));
        }

        if (!test.IsSentOut)
        {
            return Result<int>.Failure(Error.Conflict("هذا التحليل غير مهيأ للإرسال للخارج."));
        }

        var entity = _db.Set<ExternalEntity>().FirstOrDefault(e => e.Id.Value == request.ExternalLabEntityId);
        if (entity is null)
        {
            return Result<int>.Failure(Error.NotFound("الجهة الخارجية غير موجودة."));
        }

        if (entity.EntityType != EntityType.PartnerLab)
        {
            return Result<int>.Failure(Error.Conflict("الجهة المختارة ليست معملًا خارجيًا."));
        }

        if (_db.Set<SentOutSample>().Any(s => s.PatientTestId.Value == request.PatientTestId))
        {
            return Result<int>.Failure(Error.Conflict("تم إرسال هذه العينة مسبقًا."));
        }

        var costPrice = request.CostPrice ?? test.SentOutCostPrice ?? 0m;
        var patientPrice = request.PatientPrice ?? test.PatientPrice;

        SentOutSample sample;
        try
        {
            sample = SentOutSample.Create(
                SentOutSampleId.Create(0),
                patientTest.Id,
                entity.Id,
                costPrice,
                patientPrice,
                _dateTime.UtcNow);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        _db.Add(sample);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(sample.Id.Value);
    }
}
