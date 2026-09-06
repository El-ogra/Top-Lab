using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.SaveWorkGroupLogItems;

public sealed class SaveWorkGroupLogItemsCommandHandler : IRequestHandler<SaveWorkGroupLogItemsCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public SaveWorkGroupLogItemsCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(SaveWorkGroupLogItemsCommand request, CancellationToken cancellationToken)
    {
        var log = _db.Set<WorkGroupLog>().FirstOrDefault(l => l.Id.Value == request.Id);
        if (log is null)
        {
            return Result.Failure(Error.NotFound("مجموعة العمل غير موجودة"));
        }

        foreach (var testId in request.TestIds)
        {
            if (!_db.Set<Test>().Any(t => t.Id.Value == testId))
            {
                return Result.Failure(Error.Validation($"معرف التحليل غير معروف: {testId}"));
            }
        }

        var existingItems = _db.Set<WorkGroupLogItem>()
            .Where(i => i.WorkGroupLogId.Equals(log.Id))
            .ToList();

        log.ClearItems();
        foreach (var item in existingItems)
        {
            _db.Remove(item);
        }

        foreach (var testId in request.TestIds)
        {
            log.AddItem(TestId.Create(testId));
            _db.Add(WorkGroupLogItem.Create(log.Id, TestId.Create(testId)));
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}