using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetPriceListById;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PatientRegistration.Commands.CreatePatient;

public sealed class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ISender _sender;

    public CreatePatientCommandHandler(IApplicationDbContext db, ISender sender)
    {
        _db = db;
        _sender = sender;
    }

    public async Task<Result<int>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var settings = await _sender.Send(new GetSystemSettingsQuery(), cancellationToken);
        if (!settings.IsSuccess)
        {
            return Result<int>.Failure(settings.Error!);
        }

        var effectiveAccountType = request.AccountType;

        LabId? labId = null;
        if (!string.IsNullOrWhiteSpace(request.LabId))
        {
            try
            {
                labId = LabId.Create(request.LabId);
            }
            catch (ArgumentException)
            {
                return Result<int>.Failure(Error.Validation("رمز المعمل غير صالح."));
            }
        }

        ExternalEntityId? treatingDoctorId = null;
        if (request.TreatingDoctorId.HasValue)
        {
            if (!_db.Set<ExternalEntity>().Any(e => e.Id.Value == request.TreatingDoctorId.Value))
            {
                return Result<int>.Failure(Error.NotFound("الطبيب المعالج غير موجود."));
            }
            treatingDoctorId = ExternalEntityId.Create(request.TreatingDoctorId.Value);
        }

        ExternalEntityId? referralEntityId = null;
        if (request.ReferralEntityId.HasValue)
        {
            var referral = _db.Set<ExternalEntity>().FirstOrDefault(e => e.Id.Value == request.ReferralEntityId.Value);
            if (referral is null)
            {
                return Result<int>.Failure(Error.NotFound("جهة الإحالة غير موجودة."));
            }

            if (referral.EntityType == EntityType.TreatingDoctor)
            {
                return Result<int>.Failure(Error.Validation("جهة الإحالة لا يمكن أن تكون طبيبًا معالجًا."));
            }

            referralEntityId = referral.Id;
        }

        Patient patient;
        try
        {
            patient = Patient.Create(
                PatientId.Create(0),
                request.FullName,
                request.Sex,
                request.AgeValue,
                request.AgeUnit,
                request.RegistrationDateUtc,
                effectiveAccountType,
                request.IsVip,
                labId,
                request.Title,
                request.NationalId,
                request.Address,
                treatingDoctorId,
                referralEntityId,
                request.PickupDateUtc,
                request.IsFastingIndicated,
                request.FastingHours,
                request.RecentContrastImaging,
                request.Notes);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        if (labId is not null)
        {
            patient.AssignLabId(labId);
        }

        patient.SetPhoneNumbers(request.PhoneNumbers ?? Array.Empty<PatientNumberInput>());

        foreach (var conditionId in request.MedicalConditionIds ?? Array.Empty<int>())
        {
            if (!_db.Set<MedicalConditionType>().Any(m => m.Id.Value == conditionId))
            {
                return Result<int>.Failure(Error.NotFound("نوع الحالة الصحية غير موجود."));
            }
            patient.AddMedicalCondition(MedicalConditionTypeId.Create(conditionId));
        }

        // D1: registration requires at least one analysis in the same Application
        // transaction. Empty selection is a Validation failure; AddTestsToVisit
        // remains the edit-existing-visit operation only.
        var orderedTests = request.Tests ?? Array.Empty<AddTestsToVisit.AddTestInput>();
        if (orderedTests.Count == 0)
        {
            return Result<int>.Failure(Error.Validation("يجب اختيار تحليل واحد على الأقل."));
        }

        if (orderedTests.Select(t => t.TestId).Distinct().Count() != orderedTests.Count)
        {
            return Result<int>.Failure(Error.Validation("لا يمكن تكرار نفس التحليل في نفس الزيارة."));
        }

        var requestedTestIds = orderedTests.Select(t => TestId.Create(t.TestId)).ToList();
        var tests = _db.Set<Test>().Where(t => requestedTestIds.Contains(t.Id)).ToDictionary(t => t.Id, t => t);

        foreach (var req in orderedTests)
        {
            if (!tests.ContainsKey(TestId.Create(req.TestId)))
            {
                return Result<int>.Failure(Error.NotFound($"التحليل غير موجود (id={req.TestId})."));
            }
        }

        ExternalEntity? referralEntity = referralEntityId is null
            ? null
            : _db.Set<ExternalEntity>().FirstOrDefault(e => e.Id.Equals(referralEntityId));

        IReadOnlyDictionary<TestId, decimal> priceListItems = new Dictionary<TestId, decimal>();
        if (referralEntity?.PriceListId is not null)
        {
            var plResult = await _sender.Send(new GetPriceListByIdQuery(referralEntity.PriceListId.Value), cancellationToken);
            if (!plResult.IsSuccess)
            {
                return Result<int>.Failure(plResult.Error!);
            }
            priceListItems = plResult.Value!.Items.ToDictionary(i => TestId.Create(i.TestId), i => i.Price);
        }

        var groupPrices = new Dictionary<TestId, decimal>();

        _db.Add(patient);

        foreach (var req in orderedTests)
        {
            var testId = TestId.Create(req.TestId);
            var test = tests[testId];

            if (referralEntity?.PriceListId is not null && !priceListItems.ContainsKey(testId))
            {
                return Result<int>.Failure(Error.Conflict("التحليل غير موجود في قائمة أسعار الجهة المحال منها."));
            }

            var price = TestPriceResolver.Resolve(
                effectiveAccountType,
                test.PatientPrice,
                test.LabToLabPrice,
                referralEntity,
                priceListItems,
                groupPrices,
                testId);

            var pt = PatientTest.Create(
                PatientTestId.Create(0),
                patient.Id,
                testId,
                price,
                req.IsUrine,
                req.IsStool,
                req.IsBlood,
                req.IsSemen,
                req.IsCsf,
                req.IsTakenOutsideLab);

            _db.Add(pt);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(patient.Id.Value);
    }
}

internal static class DomainFailureTranslator
{
    internal static string Translate(ArgumentException ex)
    {
        return ex.ParamName switch
        {
            "fullName" => "اسم المريض مطلوب.",
            "ageValue" => "العمر يجب أن يكون صفرًا أو أكثر.",
            "fastingHours" => "ساعات الصيام تتطلب تحديد الصيام.",
            _ => "بيانات المريض غير صالحة."
        };
    }
}