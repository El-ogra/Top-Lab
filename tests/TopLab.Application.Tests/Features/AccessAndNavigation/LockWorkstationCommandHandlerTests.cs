using System.Reflection;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.AccessAndNavigation.Commands.LockWorkstation;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.AccessAndNavigation;

public class LockWorkstationCommandHandlerTests
{
    [Fact]
    public async Task Handle_CallsClearSession_ExactlyOnce()
    {
        var user = new ClearSessionSpyCurrentUserService();
        var handler = new LockWorkstationCommandHandler(user);

        var result = await handler.Handle(new LockWorkstationCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, user.ClearSessionCallCount);
    }

    [Fact]
    public void Handler_DoesNotDependOnIApplicationDbContext()
    {
        // Verified by test inspection: the handler's constructor parameters
        // never include IApplicationDbContext. LockWorkstationCommand is a
        // thin, purely in-memory command.
        var constructor = typeof(LockWorkstationCommandHandler).GetConstructors().Single();

        Assert.DoesNotContain(
            constructor.GetParameters(),
            p => p.ParameterType == typeof(IApplicationDbContext));
    }

    [Fact]
    public async Task Handle_OnAlreadyClearedSession_DoesNotThrow_AndReturnsSuccess()
    {
        var user = new FakeCurrentUserService();
        user.ClearSession();
        var handler = new LockWorkstationCommandHandler(user);

        var result = await handler.Handle(new LockWorkstationCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(user.IsAuthenticated);
    }

    [Fact]
    public async Task Handle_ReturnsResultSuccess()
    {
        var user = new FakeCurrentUserService();
        var handler = new LockWorkstationCommandHandler(user);

        var result = await handler.Handle(new LockWorkstationCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Null(result.Error);
    }
}

/// <summary>
/// Test double that counts <see cref="ICurrentUserService.ClearSession"/> calls.
/// </summary>
public sealed class ClearSessionSpyCurrentUserService : ICurrentUserService
{
    public int ClearSessionCallCount { get; private set; }

    public bool IsAuthenticated { get; private set; }

    public int UserId { get; private set; }

    public string UserName => string.Empty;

    public bool IsAbsolutePermission => false;

    public bool HasPermission(string code) => false;

    public void SetSession(int userId, string userName, bool isAbsolutePermission, IEnumerable<string> grantedPermissions)
    {
        IsAuthenticated = true;
    }

    public void ClearSession()
    {
        ClearSessionCallCount++;
        IsAuthenticated = false;
    }
}