using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.ClearAllTests;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetPriceListById;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PatientRegistration.Commands.AddTestGroupToVisit;

public sealed class AddTestGroupToVisitCommandHandler
    : IRequestHandler<AddTestGroupToVisitCommand, Result<IReadOnlyList<int>>>
{
    public const string EmptyGroupMessage = "المجموعة فارغة.";
    public const string FirstRegistrationOnlyMessage = ClearAllTestsCommandHandler.FirstRegistrationOnlyMessage;

    private readonly IApplicationDbContext _db;
    private readonly ISender _sender;
    private readonly IDateTimeProvider _clock;

    public AddTestGroupToVisitCommandHandler(IApplicationDbContext db, ISender sender, IDateTimeProvider clock)
    {
        _db = db;
        _sender = sender;
        _clock = clock;
    }

    public async Task<Result<IReadOnlyList<int>>> Handle(AddTestGroupToVisitCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null)
        {
            return Result<IReadOnlyList<int>>.Failure(Error.NotFound("المريض غير موجود."));
        }

        if (patient.IsDeleted)
        {
            return Result<IReadOnlyList<int>>.Failure(Error.Conflict("المريض محذوف."));
        }

        // First-registration rule (consistent with ClearAllTestsCommandHandler).
        var labId = patient.LabId?.Value;
        var registeredAt = patient.RegistrationDateUtc;
        var patientId = patient.Id.Value;

        var isFirstRegistration = labId is null
            || !_db.Set<Patient>()
                .Where(p => !p.IsDeleted
                    && p.Id.Value != patientId
                    && p.LabId != null
                    && p.LabId.Value == labId)
                .Any(p => p.RegistrationDateUtc < registeredAt
                    || (p.RegistrationDateUtc == registeredAt && p.Id.Value < patientId));

        if (!isFirstRegistration)
        {
            return Result<IReadOnlyList<int>>.Failure(Error.Conflict(FirstRegistrationOnlyMessage));
        }

        if ((_clock.UtcNow - patient.CreatedAtUtc).TotalHours > 24)
        {
            return Result<IReadOnlyList<int>>.Failure(Error.Conflict("لا يمكن إضافة مجموعة تحاليل من مريض أضيف قبل أكثر من 24 ساعة."));
        }

        var group = _db.Set<TestGroup>().FirstOrDefault(g => g.Id.Value == request.TestGroupId);
        if (group is null)
        {
            return Result<IReadOnlyList<int>>.Failure(Error.NotFound("المجموعة غير موجودة."));
        }

        if (!group.IsActive)
        {
            return Result<IReadOnlyList<int>>.Failure(Error.Conflict("المجموعة غير نشطة."));
        }

        // Only ACTIVE tests belonging to the group.
        var tests = _db.Set<Test>()
            .Where(t => t.TestGroupId != null && t.TestGroupId.Value == request.TestGroupId && t.IsActive)
            .ToList();

        if (tests.Count == 0)
        {
            return Result<IReadOnlyList<int>>.Failure(Error.Conflict(EmptyGroupMessage));
        }

        // Skip tests already ordered for this visit (no duplicate).
        var existingTestIds = _db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Equals(patient.Id))
            .Select(pt => pt.TestId.Value)
            .ToHashSet();

        var newTests = tests.Where(t => !existingTestIds.Contains(t.Id.Value)).ToList();
        if (newTests.Count == 0)
        {
            return Result<IReadOnlyList<int>>.Success(Array.Empty<int>());
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

        var accountType = patient.AccountType;
        var groupPrices = new Dictionary<TestId, decimal>(); // master group has no custom prices

        var createdIds = new List<int>();

        foreach (var test in newTests)
        {
            var testId = test.Id;

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
                price);

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