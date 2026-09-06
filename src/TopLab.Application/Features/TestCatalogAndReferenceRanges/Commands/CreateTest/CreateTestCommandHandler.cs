using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTest;

public sealed class CreateTestCommandHandler : IRequestHandler<CreateTestCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public CreateTestCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(CreateTestCommand request, CancellationToken cancellationToken)
    {
        if (_db.Set<Test>().Any(t => t.TestCode == request.TestCode))
        {
            return Result<int>.Failure(Error.Conflict("كود التحليل مستخدم بالفعل"));
        }

        if (_db.Set<Test>().Any(t => t.Name == request.Name))
        {
            return Result<int>.Failure(Error.Conflict("اسم التحليل مستخدم بالفعل"));
        }

        if (request.TestGroupId is int groupId
            && !_db.Set<TestGroup>().Any(g => g.Id.Value == groupId))
        {
            return Result<int>.Failure(Error.NotFound("مجموعة التحاليل غير موجودة"));
        }

        var test = Test.Create(
            TestId.Create(0),
            request.Name,
            request.ReportName,
            request.ReceiptName,
            request.TestCode,
            request.CompletionDurationMinutes,
            request.PatientPrice,
            request.ResultKind,
            request.IsCultureType,
            request.TestGroupId is int assignedGroupId ? TestGroupId.Create(assignedGroupId) : null,
            request.Barcode,
            request.IsSentOut,
            request.SentOutCostPrice,
            request.LabToLabPrice);

        _db.Add(test);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (IsUniqueViolation(ex))
        {
            return Result<int>.Failure(Error.Conflict("كود التحليل مستخدم بالفعل"));
        }

        return Result<int>.Success(test.Id.Value);
    }

    private static bool IsUniqueViolation(Exception ex)
    {
        var msg = ex.Message;
        return msg.Contains("IX_Tests_TestCode") || msg.Contains("duplicate") || msg.Contains("UNIQUE");
    }
}