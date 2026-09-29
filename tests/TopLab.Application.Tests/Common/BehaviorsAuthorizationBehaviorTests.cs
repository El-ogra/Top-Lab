using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Common;

public class BehaviorsAuthorizationBehaviorTests
{
    /// <summary>Test-local request; not product code and not a catalog permission.</summary>
    private sealed record LocalAuthProbe(string Name)
        : IRequest<Result<string>>, IAuthorizedRequest
    {
        public string RequiredPermissionCode => "TEST_LOCAL_AUTH";
    }

    private static LocalAuthProbe Sample() => new(Name: "Sara");

    [Fact]
    public async Task AuthorizationBehavior_ReturnsForbidden_WhenUserLacksPermission()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<LocalAuthProbe, Result<string>>(user);

        var response = await behavior.Handle(Sample(), _ => throw new Exception("handler must not run"), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
    }

    [Fact]
    public async Task AuthorizationBehavior_ReturnsForbidden_WhenPermissionNotGranted()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false, GrantedPermissions = { "OTHER" } };
        var behavior = new AuthorizationBehavior<LocalAuthProbe, Result<string>>(user);

        var response = await behavior.Handle(Sample(), _ => throw new Exception("handler must not run"), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", response.Error!.Message);
    }

    [Fact]
    public async Task AuthorizationBehavior_CallsNext_WhenUserHasPermission()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false, GrantedPermissions = { "TEST_LOCAL_AUTH" } };
        var behavior = new AuthorizationBehavior<LocalAuthProbe, Result<string>>(user);

        var response = await behavior.Handle(Sample(), _ => Task.FromResult(Result<string>.Success("ok")), CancellationToken.None);

        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task AuthorizationBehavior_CallsNext_WhenUserIsAbsolute()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = true };
        var behavior = new AuthorizationBehavior<LocalAuthProbe, Result<string>>(user);

        var response = await behavior.Handle(Sample(), _ => Task.FromResult(Result<string>.Success("ok")), CancellationToken.None);

        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task AuthorizationBehavior_SkipsCheck_WhenRequestNotAuthorized()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<PlainRequest, Result>(user);

        var response = await behavior.Handle(PlainRequest.Instance, _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.True(response.IsSuccess);
    }

    private sealed record PlainRequest : IRequest<Result>
    {
        public static readonly PlainRequest Instance = new();
    }
}
