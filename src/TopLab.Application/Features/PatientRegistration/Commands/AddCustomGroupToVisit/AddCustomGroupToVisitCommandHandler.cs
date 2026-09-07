using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupById;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetPriceListById;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PatientRegistration.Commands.AddCustomGroupToVisit;

public sealed class AddCustomGroupToVisitCommandHandler : IRequestHandler<AddCustomGroupToVisitCommand, Result<IReadOnlyList<int>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ISender _sender;

    public AddCustomGroupToVisitCommandHandler(IApplicationDbContext db, ISender sender)
    {
        _db = db;
        _sender = sender;
    }

    public async Task<Result<IReadOnlyList<int>>> Handle(AddCustomGroupToVisitCommand request, CancellationToken cancellationToken)
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

        var groupResult = await _sender.Send(new GetCustomGroupByIdQuery(request.CustomGroupId), cancellationToken);
        if (!groupResult.IsSuccess)
        {
            return Result<IReadOnlyList<int>>.Failure(groupResult.Error!);
        }

        var group = groupResult.Value!;
        if (group.Items.Count == 0)
        {
            return Result<IReadOnlyList<int>>.Failure(Error.Conflict("المجموعة فارغة."));
        }

        var requestedTestIds = group.Items.Select(i => TestId.Create(i.TestId)).ToList();
        var tests = _db.Set<Test>().Where(t => requestedTestIds.Contains(t.Id)).ToDictionary(t => t.Id, t => t);

        foreach (var item in group.Items)
        {
            if (!tests.ContainsKey(TestId.Create(item.TestId)))
            {
                return Result<IReadOnlyList<int>>.Failure(Error.NotFound($"التحليل غير موجود (id={item.TestId})."));
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

        var groupPrices = group.Items.ToDictionary(i => TestId.Create(i.TestId), i => i.Price);
        var accountType = patient.AccountType;

        var createdIds = new List<int>();

        foreach (var item in group.Items)
        {
            var testId = TestId.Create(item.TestId);
            var test = tests[testId];

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