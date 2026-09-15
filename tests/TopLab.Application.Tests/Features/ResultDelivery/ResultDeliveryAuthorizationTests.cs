using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryAccount;
using TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryGrid;
using TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultDelivery;

public class ResultDeliveryAuthorizationTests
{
    [Theory]
    [InlineData(typeof(GetUndeliveredResultsQuery))]
    [InlineData(typeof(GetDeliveryGridQuery))]
    [InlineData(typeof(GetDeliveryAccountQuery))]
    public void Queries_Require_DeliverResults(Type queryType)
    {
        IAuthorizedRequest authorized = queryType switch
        {
            _ when queryType == typeof(GetUndeliveredResultsQuery) => new GetUndeliveredResultsQuery(),
            _ when queryType == typeof(GetDeliveryGridQuery) => new GetDeliveryGridQuery(1),
            _ when queryType == typeof(GetDeliveryAccountQuery) => new GetDeliveryAccountQuery(1),
            _ => throw new InvalidOperationException("Unknown query type."),
        };

        Assert.Equal("DELIVER_RESULTS", authorized.RequiredPermissionCode);
        Assert.Equal(ResultDeliveryAccessPolicy.DeliverResults, authorized.RequiredPermissionCode);
    }

    [Fact]
    public async Task WithoutPermission_ReturnsForbidden_WithSharedMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false, GrantedPermissions = { } };
        var behavior = new AuthorizationBehavior<GetDeliveryGridQuery, Result<IReadOnlyList<DeliveryGridRowDto>>>(user);

        var response = await behavior.Handle(
            new GetDeliveryGridQuery(1),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, response.Error!.Type);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", response.Error.Message);
    }

    [Fact]
    public async Task AbsolutePermission_BypassesCheck()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = true };
        var behavior = new AuthorizationBehavior<GetDeliveryAccountQuery, Result<DeliveryAccountDto>>(user);

        var called = false;
        var response = await behavior.Handle(
            new GetDeliveryAccountQuery(1),
            _ =>
            {
                called = true;
                return Task.FromResult(Result<DeliveryAccountDto>.Success(
                    new DeliveryAccountDto(1, 0m, 0m, 0m, 0m, 0m)));
            },
            CancellationToken.None);

        Assert.True(called);
        Assert.True(response.IsSuccess);
    }
}
