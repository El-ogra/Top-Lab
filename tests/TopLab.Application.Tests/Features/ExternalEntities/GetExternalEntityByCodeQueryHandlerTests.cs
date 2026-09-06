using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Queries.GetExternalEntityByCode;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using Xunit;

namespace TopLab.Application.Tests.Features.ExternalEntities;

public class GetExternalEntityByCodeQueryHandlerTests
{
    private static FakeApplicationDbContext BuildDb()
    {
        var db = new FakeApplicationDbContext();
        var lab = ExternalEntity.Create(
            ExternalEntityId.Create(3), EntityType.PartnerLab, "Alpha Lab");
        lab.RegenerateIdCode("LAB001");
        db.Add(lab);
        db.Add(ExternalEntity.Create(
            ExternalEntityId.Create(4), EntityType.TreatingDoctor, "Dr. Ahmed"));
        return db;
    }

    [Fact]
    public async Task GetByCode_KnownCode_ReturnsEntity()
    {
        var handler = new GetExternalEntityByCodeQueryHandler(BuildDb());

        var result = await handler.Handle(new GetExternalEntityByCodeQuery("LAB001"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Alpha Lab", result.Value!.Name);
        Assert.Equal("LAB001", result.Value.GeneratedIdCode);
    }

    [Fact]
    public async Task GetByCode_UnknownCode_ReturnsNotFound()
    {
        var handler = new GetExternalEntityByCodeQueryHandler(BuildDb());

        var result = await handler.Handle(new GetExternalEntityByCodeQuery("NOPE"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}
