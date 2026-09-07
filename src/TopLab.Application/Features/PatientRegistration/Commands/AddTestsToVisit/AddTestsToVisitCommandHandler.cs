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

namespace TopLab.Application.Features.PatientRegistration.Commands.AddTestsToVisit;

public sealed class AddTestsToVisitCommandHandler : IRequestHandler<AddTestsToVisitCommand, Result<IReadOnlyList<int>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ISender _sender;

    public AddTestsToVisitCommandHandler(IApplicationDbContext db, ISender sender)
    {
        _db = db;
        _sender = sender;
    }

    public async Task<Result<IReadOnlyList<int>>> Handle(AddTestsToVisitCommand request, CancellationToken cancellationToken)
    {
        if (request.Tests.Select(t => t.TestId).Distinct().Count() != request.Tests.Count)
        {
            return Result<IReadOnlyList<int>>.Failure(Error.Validation("لا يمكن تكرار نفس التحليل في نفس الزيارة."));
        }

        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null)
        {
            return Result<IReadOnlyList<int>>.Failure(Error.NotFound("المريض غير موجود."));
        }

        if (patient.IsDeleted)
        {
            return Result<IReadOnlyList<int>>.Failure(Error.Conflict("المريض محذوف ولا يمكن إضافة تحاليل إليه."));
        }

        var requestedTestIds = request.Tests.Select(t => TestId.Create(t.TestId)).ToList();
        var tests = _db.Set<Test>().Where(t => requestedTestIds.Contains(t.Id)).ToDictionary(t => t.Id, t => t);

        foreach (var req in request.Tests)
        {
            if (!tests.ContainsKey(TestId.Create(req.TestId)))
            {
                return Result<IReadOnlyList<int>>.Failure(Error.NotFound($"التحليل غير موجود (id={req.TestId})."));
            }
        }

        ExternalEntity? referralEntity = patient.ReferralEntityId is null
            ? null
            : _db.Set<ExternalEntity>().FirstOrDefault(e => e.Id.Equals(patient.ReferralEntityId));

        IReadOnlyDictionary<TestId, decimal> priceListItems = new Dictionary<TestId, decimal>();
        if (referralEntity?.PriceListId is not null)
        {
            var plResult = await _sender.Send(new GetPriceListByIdQuery(referralEntity.PriceListId.Value), cancellationToken);
            if (!plResult.IsSuccess)
            {
                return Result<IReadOnlyList<int>>.Failure(plResult.Error!);
            }
            priceListItems = plResult.Value!.Items.ToDictionary(i => TestId.Create(i.TestId), i => i.Price);
        }

        var groupPrices = new Dictionary<TestId, decimal>();
        var accountType = patient.AccountType;

        var createdIds = new List<int>();

        foreach (var req in request.Tests)
        {
            var testId = TestId.Create(req.TestId);
            var test = tests[testId];

            if (referralEntity?.PriceListId is not null && !priceListItems.ContainsKey(testId))
            {
                return Result<IReadOnlyList<int>>.Failure(Error.Conflict("التحليل غير موجود في قائمة أسعار الجهة المحال منها."));
            }

            var price = TestPriceResolver.Resolve(
                accountType,
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
            createdIds.Add(0);
        }

        await _db.SaveChangesAsync(cancellationToken);

        var newIds = _db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Equals(patient.Id))
            .OrderByDescending(pt => pt.Id.Value)
            .Take(createdIds.Count)
            .Select(pt => pt.Id.Value)
            .ToList();

        return Result<IReadOnlyList<int>>.Success(newIds);
    }
}