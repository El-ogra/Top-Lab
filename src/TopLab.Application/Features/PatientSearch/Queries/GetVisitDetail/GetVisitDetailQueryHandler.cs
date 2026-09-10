using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientSearch.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PatientSearch.Queries.GetVisitDetail;

/// <summary>
/// Full read model of a single visit (registration): demographics, phone numbers ordered
/// by SortOrder, medical-condition names resolved via the join + MedicalConditionTypes
/// (ordered by MedicalConditionTypeId), test lines ordered by PatientTestId, and the
/// per-visit balance (BalanceProbe).
/// </summary>
public sealed class GetVisitDetailQueryHandler
    : IRequestHandler<GetVisitDetailQuery, Result<VisitDetailDto>>
{
    private readonly IApplicationDbContext _db;

    public GetVisitDetailQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<VisitDetailDto>> Handle(
        GetVisitDetailQuery request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>()
            .FirstOrDefault(p => p.Id.Value == request.PatientId);

        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<VisitDetailDto>.Failure(
                Error.NotFound("المريض غير موجود.")));
        }

        var testLines = _db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Value == request.PatientId)
            .OrderBy(pt => pt.Id.Value)
            .ToList();

        var testIds = testLines.Select(pt => pt.TestId).Distinct().ToList();
        var tests = _db.Set<Test>()
            .Where(t => testIds.Contains(t.Id))
            .ToDictionary(t => t.Id, t => t);

        var phoneNumbers = _db.Set<PatientPhoneNumber>()
            .Where(ph => ph.PatientId.Value == request.PatientId)
            .OrderBy(ph => ph.SortOrder)
            .Select(ph => ph.PhoneNumber)
            .ToList();

        var conditionTypeIds = _db.Set<PatientMedicalCondition>()
            .Where(pm => pm.PatientId.Value == request.PatientId)
            .OrderBy(pm => pm.MedicalConditionTypeId.Value)
            .Select(pm => pm.MedicalConditionTypeId)
            .ToList();
        var medicalConditionTypes = _db.Set<MedicalConditionType>()
            .Where(mt => conditionTypeIds.Contains(mt.Id))
            .ToDictionary(mt => mt.Id, mt => mt.Name);
        var conditionNames = conditionTypeIds
            .Select(id => medicalConditionTypes.TryGetValue(id, out var name) ? name : string.Empty)
            .ToList();

        var lines = testLines
            .Select(pt =>
            {
                tests.TryGetValue(pt.TestId, out var test);
                return new VisitTestLineDto(
                    pt.Id.Value,
                    test?.Name ?? string.Empty,
                    test?.TestCode ?? string.Empty,
                    pt.PriceAtOrderTime,
                    pt.IsSampleDrawn,
                    pt.IsTakenOutsideLab,
                    pt.ResultValue,
                    pt.ResultFlag is null ? null : (int)pt.ResultFlag.Value,
                    pt.IsReviewed,
                    pt.IsPrinted,
                    pt.PrintCount,
                    pt.IsDelivered);
            })
            .ToList();

        var dto = new VisitDetailDto(
            patient.Id.Value,
            patient.FullName,
            patient.Title,
            patient.Sex.ToString(),
            patient.AgeValue,
            patient.AgeUnit.ToString(),
            patient.NationalId,
            patient.Address,
            patient.AccountType.ToString(),
            patient.IsVip,
            patient.RegistrationDateUtc,
            patient.PickupDateUtc,
            phoneNumbers,
            conditionNames,
            lines,
            BalanceProbe.Balance(_db, patient.Id.Value));

        return Task.FromResult(Result<VisitDetailDto>.Success(dto));
    }
}