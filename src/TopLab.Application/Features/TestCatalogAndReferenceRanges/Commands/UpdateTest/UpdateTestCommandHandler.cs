using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateTest;

public sealed class UpdateTestCommandHandler : IRequestHandler<UpdateTestCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateTestCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(UpdateTestCommand request, CancellationToken cancellationToken)
    {
        var test = _db.Set<Test>().FirstOrDefault(t => t.Id.Value == request.Id);
        if (test is null)
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود"));
        }

        if (request.TestGroupId is int groupId
            && !_db.Set<TestGroup>().Any(g => g.Id.Value == groupId))
        {
            return Result.Failure(Error.NotFound("مجموعة التحاليل غير موجودة"));
        }

        test.Update(
            request.Name,
            request.ReportName,
            request.ReceiptName,
            request.TestCode,
            request.CompletionDurationMinutes,
            request.PatientPrice,
            request.TestGroupId is int assignedGroupId ? TestGroupId.Create(assignedGroupId) : null,
            request.Barcode,
            request.IsSentOut,
            request.SentOutCostPrice,
            request.LabToLabPrice);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (IsUniqueViolation(ex))
        {
            return Result.Failure(Error.Conflict("كود التحليل مستخدم بالفعل"));
        }

        return Result.Success();
    }

    private static bool IsUniqueViolation(Exception ex)
    {
        var msg = ex.Message;
        return msg.Contains("IX_Tests_TestCode") || msg.Contains("duplicate") || msg.Contains("UNIQUE");
    }
}