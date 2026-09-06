using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestById;

public sealed class GetTestByIdQueryHandler : IRequestHandler<GetTestByIdQuery, Result<TestDetailDto>>
{
    private readonly IApplicationDbContext _db;

    public GetTestByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<TestDetailDto>> Handle(GetTestByIdQuery request, CancellationToken cancellationToken)
    {
        var test = _db.Set<Test>().FirstOrDefault(t => t.Id.Value == request.TestId);
        if (test is null)
        {
            return Task.FromResult(Result<TestDetailDto>.Failure(Error.NotFound("التحليل غير موجود")));
        }

        var groupName = test.TestGroupId is not null
            ? _db.Set<TestGroup>().FirstOrDefault(g => g.Id.Equals(test.TestGroupId))?.Name
            : null;

        var dto = new TestDetailDto(
            test.Id.Value,
            test.TestCode,
            test.Name,
            test.ReportName,
            test.ReceiptName,
            test.TestGroupId?.Value,
            groupName,
            test.Barcode,
            test.CompletionDurationMinutes,
            test.IsSentOut,
            test.SentOutCostPrice,
            test.PatientPrice,
            test.LabToLabPrice,
            (int)test.ResultKind,
            test.IsCultureType,
            test.IsActive);

        return Task.FromResult(Result<TestDetailDto>.Success(dto));
    }
}