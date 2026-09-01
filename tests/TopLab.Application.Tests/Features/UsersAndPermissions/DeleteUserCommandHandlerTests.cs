using TopLab.Application.Features.UsersAndPermissions.Commands.DeleteUser;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Accounting;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public class DeleteUserCommandHandlerTests
{
    [Fact]
    public async Task Delete_ReferencedUser_ReturnsConflict_AndNotRemoved()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var user = User.Create(UserId.Create(5), "referenced", hasher.Hash("p"), hasher.Hash("s"));
        db.Users.Add(user);

        var patient = Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        // Simulate audit reference by setting CreatedByUserId via interceptor simulation
        patient.CreatedByUserId = 5;
        db.Patients.Add(patient);

        var handler = new DeleteUserCommandHandler(db);
        var result = await handler.Handle(new DeleteUserCommand(5), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن حذف مستخدم له سجلات مرتبطة؛ استخدم التعطيل بدلاً من الحذف", result.Error!.Message);
        Assert.Contains(db.Users, u => u.Id.Value == 5);
    }

    [Fact]
    public async Task Delete_NeverUsedLimitedUser_Removed()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var user = User.Create(UserId.Create(10), "temp", hasher.Hash("p"), hasher.Hash("s"), isAbsolutePermission: false);
        db.Users.Add(user);

        var handler = new DeleteUserCommandHandler(db);
        var result = await handler.Handle(new DeleteUserCommand(10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(db.Users, u => u.Id.Value == 10);
    }

    [Fact]
    public async Task Delete_LastActiveAbsolute_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var admin = User.Create(UserId.Create(1), "admin", hasher.Hash("p"), hasher.Hash("s"), true);
        db.Users.Add(admin);

        var handler = new DeleteUserCommandHandler(db);
        var result = await handler.Handle(new DeleteUserCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن تعطيل آخر مدير نظام؛ يجب إنشاء بديل أولاً", result.Error!.Message);
        Assert.Contains(db.Users, u => u.Id.Value == 1);
    }

    [Fact]
    public async Task Delete_ReferencedByCashMovement_PerformedByUserId_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var user = User.Create(UserId.Create(7), "cashuser", hasher.Hash("p"), hasher.Hash("s"));
        db.Users.Add(user);

        var cash = CashMovement.Create(CashMovementId.Create(1), MovementType.Deposit, 100, 7, DateTime.UtcNow);
        db.CashMovements.Add(cash);

        var handler = new DeleteUserCommandHandler(db);
        var result = await handler.Handle(new DeleteUserCommand(7), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Conflict, result.Error!.Type);
    }
}
