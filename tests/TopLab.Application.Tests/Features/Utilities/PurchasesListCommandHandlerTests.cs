using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Commands.AddPhoneBookEntry;
using TopLab.Application.Features.Utilities.Commands.AddPurchaseItem;
using TopLab.Application.Features.Utilities.Commands.RemovePhoneBookEntry;
using TopLab.Application.Features.Utilities.Commands.RemovePurchaseItem;
using TopLab.Application.Features.Utilities.Commands.TogglePurchaseItemDone;
using TopLab.Application.Features.Utilities.Common;
using TopLab.Application.Features.Utilities.Queries.GetPhoneBook;
using TopLab.Application.Features.Utilities.Queries.GetPurchasesList;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.Utilities;

public sealed class FakePurchasesListStore : IPurchasesListStore
{
    public List<PurchaseItemDto> Items { get; } = new();

    public Task<Result<IReadOnlyList<PurchaseItemDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Result<IReadOnlyList<PurchaseItemDto>>.Success(Items.ToList()));

    public Task<Result> SaveAllAsync(IReadOnlyList<PurchaseItemDto> items, CancellationToken cancellationToken = default)
    {
        Items.Clear();
        Items.AddRange(items);
        return Task.FromResult(Result.Success());
    }
}

public class PurchasesListCommandHandlerTests
{
    [Fact]
    public async Task Add_AppendsWithNextIdAndTimestamp()
    {
        var store = new FakePurchasesListStore();
        var clock = new FakeDateTimeProvider();
        var handler = new AddPurchaseItemCommandHandler(store, clock);

        await handler.Handle(new AddPurchaseItemCommand("كحول"), CancellationToken.None);
        await handler.Handle(new AddPurchaseItemCommand("قفازات"), CancellationToken.None);

        Assert.Equal(2, store.Items.Count);
        Assert.Equal(1, store.Items[0].Id);
        Assert.Equal(2, store.Items[1].Id);
        Assert.Equal("كحول", store.Items[0].Text);
        Assert.False(store.Items[0].IsDone);
        Assert.Equal(clock.UtcNow, store.Items[0].CreatedAtUtc);
    }

    [Fact]
    public async Task Remove_RemovesById_MissingReturnsNotFound()
    {
        var store = new FakePurchasesListStore();
        store.Items.Add(new PurchaseItemDto(1, "a", false, DateTime.UtcNow));
        var handler = new RemovePurchaseItemCommandHandler(store);

        var missing = await handler.Handle(new RemovePurchaseItemCommand(99), CancellationToken.None);
        Assert.False(missing.IsSuccess);

        var removed = await handler.Handle(new RemovePurchaseItemCommand(1), CancellationToken.None);
        Assert.True(removed.IsSuccess);
        Assert.Empty(store.Items);
    }

    [Fact]
    public async Task Toggle_FlipsIsDone()
    {
        var store = new FakePurchasesListStore();
        store.Items.Add(new PurchaseItemDto(1, "a", false, DateTime.UtcNow));
        var handler = new TogglePurchaseItemDoneCommandHandler(store);

        await handler.Handle(new TogglePurchaseItemDoneCommand(1), CancellationToken.None);
        Assert.True(store.Items[0].IsDone);

        await handler.Handle(new TogglePurchaseItemDoneCommand(1), CancellationToken.None);
        Assert.False(store.Items[0].IsDone);
    }

    [Fact]
    public async Task Get_OrdersById()
    {
        var store = new FakePurchasesListStore();
        store.Items.Add(new PurchaseItemDto(3, "c", false, DateTime.UtcNow));
        store.Items.Add(new PurchaseItemDto(1, "a", true, DateTime.UtcNow));
        var handler = new GetPurchasesListQueryHandler(store);

        var result = await handler.Handle(new GetPurchasesListQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 3 }, result.Value!.Select(i => i.Id).ToArray());
    }

    [Fact]
    public void Validator_RejectsEmptyText_WithFrozenMessage()
    {
        var validator = new AddPurchaseItemCommandValidator();
        var outcome = validator.Validate(new AddPurchaseItemCommand(""));
        Assert.False(outcome.IsValid);
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "نص البند مطلوب.");
    }
}

public class PhoneBookCommandHandlerTests
{
    [Fact]
    public async Task AddAndRemove_OverFakeStore()
    {
        var store = new FakePhoneBookStore();
        var add = new AddPhoneBookEntryCommandHandler(store);
        var remove = new RemovePhoneBookEntryCommandHandler(store);

        await add.Handle(new AddPhoneBookEntryCommand("مورد", "0770000000", "ملاحظة"), CancellationToken.None);
        Assert.Single(store.Entries);
        Assert.Equal("مورد", store.Entries[0].Name);

        var missing = await remove.Handle(new RemovePhoneBookEntryCommand(9), CancellationToken.None);
        Assert.False(missing.IsSuccess);

        var removed = await remove.Handle(new RemovePhoneBookEntryCommand(1), CancellationToken.None);
        Assert.True(removed.IsSuccess);
        Assert.Empty(store.Entries);
    }

    [Fact]
    public async Task Get_OrdersById()
    {
        var store = new FakePhoneBookStore();
        store.Entries.Add(new PhoneBookEntryDto(2, "B", "2", null));
        store.Entries.Add(new PhoneBookEntryDto(1, "A", "1", null));
        var handler = new GetPhoneBookQueryHandler(store);

        var result = await handler.Handle(new GetPhoneBookQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 2 }, result.Value!.Select(e => e.Id).ToArray());
    }

    [Fact]
    public void Validator_RejectsBlankNameAndPhone_WithFrozenMessages()
    {
        var validator = new AddPhoneBookEntryCommandValidator();
        var outcome = validator.Validate(new AddPhoneBookEntryCommand("", "", null));
        Assert.False(outcome.IsValid);
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "الاسم مطلوب.");
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "رقم الهاتف مطلوب.");
    }
}

public sealed class FakePhoneBookStore : IPhoneBookStore
{
    public List<PhoneBookEntryDto> Entries { get; } = new();

    public Task<Result<IReadOnlyList<PhoneBookEntryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Result<IReadOnlyList<PhoneBookEntryDto>>.Success(Entries.ToList()));

    public Task<Result> SaveAllAsync(IReadOnlyList<PhoneBookEntryDto> entries, CancellationToken cancellationToken = default)
    {
        Entries.Clear();
        Entries.AddRange(entries);
        return Task.FromResult(Result.Success());
    }
}
