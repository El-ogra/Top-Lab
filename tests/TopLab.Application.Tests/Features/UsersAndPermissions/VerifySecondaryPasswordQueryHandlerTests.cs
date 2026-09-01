using TopLab.Application.Features.UsersAndPermissions.Queries.VerifySecondaryPassword;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public class VerifySecondaryPasswordQueryHandlerTests
{
    [Fact]
    public async Task UnauthenticatedSession_ReturnsFailure()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var currentUser = new FakeCurrentUserService { IsAuthenticated = false };
        var handler = new VerifySecondaryPasswordQueryHandler(db, hasher, currentUser);

        var result = await handler.Handle(new VerifySecondaryPasswordQuery("any"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task WrongValue_ReturnsSuccessFalse()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var hash = hasher.Hash("correct_secondary");
        var user = User.Create(UserId.Create(5), "ahmed", hasher.Hash("main"), hash);
        db.Users.Add(user);

        var currentUser = new FakeCurrentUserService { IsAuthenticated = true, UserId = 5, UserName = "ahmed" };
        var handler = new VerifySecondaryPasswordQueryHandler(db, hasher, currentUser);

        var result = await handler.Handle(new VerifySecondaryPasswordQuery("wrong"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
    }

    [Fact]
    public async Task CorrectSecondaryPassword_ReturnsSuccessTrue()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var hash = hasher.Hash("correct_secondary");
        var user = User.Create(UserId.Create(5), "ahmed", hasher.Hash("main"), hash);
        db.Users.Add(user);

        var currentUser = new FakeCurrentUserService { IsAuthenticated = true, UserId = 5, UserName = "ahmed" };
        var handler = new VerifySecondaryPasswordQueryHandler(db, hasher, currentUser);

        var result = await handler.Handle(new VerifySecondaryPasswordQuery("correct_secondary"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task Verification_UsesSessionUsersOwnHash_NotOtherUser()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();

        var hashUser5 = hasher.Hash("sec5");
        var hashUser6 = hasher.Hash("sec6");
        var user5 = User.Create(UserId.Create(5), "user5", hasher.Hash("main5"), hashUser5);
        var user6 = User.Create(UserId.Create(6), "user6", hasher.Hash("main6"), hashUser6);
        db.Users.Add(user5);
        db.Users.Add(user6);

        var currentUser = new FakeCurrentUserService { IsAuthenticated = true, UserId = 5, UserName = "user5" };
        var handler = new VerifySecondaryPasswordQueryHandler(db, hasher, currentUser);

        // Try to verify with user6's secondary password while session is user5
        var resultWithOther = await handler.Handle(new VerifySecondaryPasswordQuery("sec6"), CancellationToken.None);
        Assert.True(resultWithOther.IsSuccess);
        Assert.False(resultWithOther.Value);

        // Correct for user5 should succeed
        var resultWithOwn = await handler.Handle(new VerifySecondaryPasswordQuery("sec5"), CancellationToken.None);
        Assert.True(resultWithOwn.IsSuccess);
        Assert.True(resultWithOwn.Value);
    }
}
