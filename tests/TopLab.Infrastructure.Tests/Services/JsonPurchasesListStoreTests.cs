using TopLab.Application.Features.Utilities.Common;
using TopLab.Infrastructure.Services;
using Xunit;

namespace TopLab.Infrastructure.Tests.Services;

public class JsonPurchasesListStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "toplab-m23-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }

    [Fact]
    public async Task RoundTrip_SaveThenLoad()
    {
        var store = new JsonPurchasesListStore(_dir);
        var items = new List<PurchaseItemDto>
        {
            new(1, "كحول", false, new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc)),
            new(2, "قفازات", true, new DateTime(2026, 3, 15, 11, 0, 0, DateTimeKind.Utc)),
        };

        var save = await store.SaveAllAsync(items);
        Assert.True(save.IsSuccess);

        var load = await store.GetAllAsync();
        Assert.True(load.IsSuccess);
        Assert.Equal(2, load.Value!.Count);
        Assert.Equal("كحول", load.Value[0].Text);
        Assert.True(load.Value[1].IsDone);
    }

    [Fact]
    public async Task MissingFile_ReturnsEmptyList()
    {
        var store = new JsonPurchasesListStore(_dir);
        var load = await store.GetAllAsync();

        Assert.True(load.IsSuccess);
        Assert.Empty(load.Value!);
    }

    [Fact]
    public async Task MalformedFile_ReturnsUnexpected()
    {
        Directory.CreateDirectory(_dir);
        await File.WriteAllTextAsync(Path.Combine(_dir, "purchases-list.json"), "{ not json");
        var store = new JsonPurchasesListStore(_dir);

        var load = await store.GetAllAsync();

        Assert.False(load.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Unexpected, load.Error!.Type);
    }
}

public class JsonPhoneBookStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "toplab-m23-phone-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }

    [Fact]
    public async Task RoundTrip_SaveThenLoad()
    {
        var store = new JsonPhoneBookStore(_dir);
        var entries = new List<PhoneBookEntryDto>
        {
            new(1, "مورد", "0770000000", "ملاحظة"),
        };

        Assert.True((await store.SaveAllAsync(entries)).IsSuccess);
        var load = await store.GetAllAsync();

        Assert.True(load.IsSuccess);
        Assert.Single(load.Value!);
        Assert.Equal("مورد", load.Value![0].Name);
        Assert.Equal("0770000000", load.Value![0].Phone);
    }

    [Fact]
    public async Task MissingFile_ReturnsEmptyList()
    {
        var store = new JsonPhoneBookStore(_dir);
        var load = await store.GetAllAsync();

        Assert.True(load.IsSuccess);
        Assert.Empty(load.Value!);
    }

    [Fact]
    public async Task MalformedFile_ReturnsUnexpected()
    {
        Directory.CreateDirectory(_dir);
        await File.WriteAllTextAsync(Path.Combine(_dir, "phone-book.json"), "[oops");
        var store = new JsonPhoneBookStore(_dir);

        var load = await store.GetAllAsync();

        Assert.False(load.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Unexpected, load.Error!.Type);
    }
}
