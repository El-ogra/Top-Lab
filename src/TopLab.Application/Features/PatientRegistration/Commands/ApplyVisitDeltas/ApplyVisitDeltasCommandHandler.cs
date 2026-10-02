using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.AddTestsToVisit;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetPriceListById;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PatientRegistration.Commands.ApplyVisitDeltas;

/// <summary>
/// W-02 S14 (WP-29): applies removals, additions and flag updates inside one
/// <see cref="IAppUnitOfWork"/> transaction with a single save. Every check runs
/// before any write, so a failure leaves the visit untouched.
/// </summary>
public sealed class ApplyVisitDeltasCommandHandler : IRequestHandler<ApplyVisitDeltasCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ISender _sender;
    private readonly IAppUnitOfWork _uow;

    public ApplyVisitDeltasCommandHandler(IApplicationDbContext db, ISender sender, IAppUnitOfWork uow)
    {
        _db = db;
        _sender = sender;
        _uow = uow;
    }

    public Task<Result> Handle(ApplyVisitDeltasCommand request, CancellationToken cancellationToken) =>
        _uow.ExecuteAsync(async ct =>
        {
            var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
            if (patient is null)
            {
                return Result.Failure(Error.NotFound("المريض غير موجود."));
            }

            if (patient.IsDeleted)
            {
                return Result.Failure(Error.Conflict("المريض محذوف ولا يمكن تعديل زيارته."));
            }

            // Phase 1 — removals (same guard as RemoveTestFromVisitCommandHandler).
            var toRemove = new List<PatientTest>();
            foreach (var id in request.RemovedPatientTestIds.Distinct())
            {
                var pt = _db.Set<PatientTest>().FirstOrDefault(t => t.Id.Value == id);
                if (pt is null)
                {
                    return Result.Failure(Error.NotFound("التحليل غير موجود."));
                }

                if (pt.EnteredAtUtc.HasValue || pt.ResultValue is not null || pt.ResultFlag.HasValue)
                {
                    return Result.Failure(Error.Conflict("لا يمكن حذف تحليل تم تسجيل نتيجته."));
                }

                toRemove.Add(pt);
            }

            // Phase 2 — additions (same validation as AddTestsToVisitCommandHandler).
            var addedInputs = request.AddedTests.ToList();
            if (addedInputs.Select(t => t.TestId).Distinct().Count() != addedInputs.Count)
            {
                return Result.Failure(Error.Validation("لا يمكن تكرار نفس التحليل في نفس الزيارة."));
            }

            var requestedTestIds = addedInputs.Select(t => TestId.Create(t.TestId)).ToList();
            var tests = requestedTestIds.Count == 0
                ? new Dictionary<TestId, Test>()
                : _db.Set<Test>().Where(t => requestedTestIds.Contains(t.Id)).ToDictionary(t => t.Id, t => t);

            foreach (var req in addedInputs)
            {
                if (!tests.ContainsKey(TestId.Create(req.TestId)))
                {
                    return Result.Failure(Error.NotFound($"التحليل غير موجود (id={req.TestId})."));
                }
            }

            ExternalEntity? referralEntity = patient.ReferralEntityId is null
                ? null
                : _db.Set<ExternalEntity>().FirstOrDefault(e => e.Id.Equals(patient.ReferralEntityId));

            IReadOnlyDictionary<TestId, decimal> priceListItems = new Dictionary<TestId, decimal>();
            if (referralEntity?.PriceListId is not null)
            {
                var plResult = await _sender.Send(new GetPriceListByIdQuery(referralEntity.PriceListId.Value), ct);
                if (!plResult.IsSuccess)
                {
                    return Result.Failure(plResult.Error!);
                }

                priceListItems = plResult.Value!.Items.ToDictionary(i => TestId.Create(i.TestId), i => i.Price);
            }

            var groupPrices = new Dictionary<TestId, decimal>();
            var accountType = patient.AccountType;

            // Phase 3 — flag updates (same write as UpdatePatientTestSampleFlagsCommandHandler).
            var flagTargets = new List<(PatientTest Pt, VisitFlagUpdate Update)>();
            foreach (var update in request.FlagUpdates)
            {
                var pt = _db.Set<PatientTest>().FirstOrDefault(t => t.Id.Value == update.PatientTestId);
                if (pt is null)
                {
                    return Result.Failure(Error.NotFound("التحليل غير موجود."));
                }

                flagTargets.Add((pt, update));
            }

            // All checks passed — apply everything, then save once.
            foreach (var pt in toRemove)
            {
                _db.Remove(pt);
            }

            foreach (var req in addedInputs)
            {
                var testId = TestId.Create(req.TestId);
                var test = tests[testId];

                if (referralEntity?.PriceListId is not null && !priceListItems.ContainsKey(testId))
                {
                    return Result.Failure(Error.Conflict("التحليل غير موجود في قائمة أسعار الجهة المحال منها."));
                }

                var resolvedPrice = TestPriceResolver.Resolve(
                    accountType,
                    test.PatientPrice,
                    test.LabToLabPrice,
                    referralEntity,
                    priceListItems,
                    groupPrices,
                    testId);
                var price = PatientAccountCalculator.ManualSelectionCharge(new[] { resolvedPrice });

                _db.Add(PatientTest.Create(
                    PatientTestId.Create(0),
                    patient.Id,
                    testId,
                    price,
                    req.IsUrine,
                    req.IsStool,
                    req.IsBlood,
                    req.IsSemen,
                    req.IsCsf,
                    req.IsTakenOutsideLab));
            }

            foreach (var (pt, update) in flagTargets)
            {
                pt.UpdateSampleFlags(
                    update.IsUrine,
                    update.IsStool,
                    update.IsBlood,
                    update.IsSemen,
                    update.IsCsf,
                    update.IsTakenOutsideLab);
            }

            await _db.SaveChangesAsync(ct);
            return Result.Success();
        }, cancellationToken);
}
