using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Settings;
using TopLab.Application.Features.SentOutSamples.Commands.SetSentOutDefaultLab;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.SentOutSamples;

public class SetSentOutDefaultLabCommandHandlerTests
{
    private static ExternalEntity PartnerLab(int id) => ExternalEntity.Create(
        ExternalEntityId.Create(id), EntityType.PartnerLab, $"مختبر {id}");

    private static ExternalEntity NonPartnerLab(int id) => ExternalEntity.Create(
        ExternalEntityId.Create(id), (EntityType)(-1), $"كيان غير صالح {id}");

    [Fact]
    public async Task HappyPath_PersistsDefaultLab()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.ExternalEntities.Add(PartnerLab(7));

        var handler = new SetSentOutDefaultLabCommandHandler(db);
        var result = await handler.Handle(new SetSentOutDefaultLabCommand(7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, db.SystemSettings[0].DefaultSentOutLabEntityId);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task Null_ClearsDefaultLab()
    {
        var db = new FakeApplicationDbContext();
        var existing = SystemSettings.CreateDefault();
        existing.SetDefaultSentOutLab(7);
        db.SystemSettings.Add(existing);

        var handler = new SetSentOutDefaultLabCommandHandler(db);
        var result = await handler.Handle(new SetSentOutDefaultLabCommand(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(db.SystemSettings[0].DefaultSentOutLabEntityId);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task MissingEntity_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());

        var handler = new SetSentOutDefaultLabCommandHandler(db);
        var result = await handler.Handle(new SetSentOutDefaultLabCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task NonPartnerLabEntity_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.ExternalEntities.Add(NonPartnerLab(3));

        var handler = new SetSentOutDefaultLabCommandHandler(db);
        var result = await handler.Handle(new SetSentOutDefaultLabCommand(3), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public void Requires_CashDisburseDeposit()
    {
        var authorized = (IAuthorizedRequest)new SetSentOutDefaultLabCommand(7);
        Assert.Equal("CASH_DISBURSE_DEPOSIT", authorized.RequiredPermissionCode);
    }

    [Fact]
    public async Task WithoutPermission_ReturnsForbidden()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<SetSentOutDefaultLabCommand, Result>(user);

        var response = await behavior.Handle(
            new SetSentOutDefaultLabCommand(7),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", response.Error.Message);
    }

    [Fact]
    public async Task AbsoluteUser_BypassesPermissionCheck()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = true };
        var behavior = new AuthorizationBehavior<SetSentOutDefaultLabCommand, Result>(user);

        var response = await behavior.Handle(
            new SetSentOutDefaultLabCommand(7),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
    }
}
