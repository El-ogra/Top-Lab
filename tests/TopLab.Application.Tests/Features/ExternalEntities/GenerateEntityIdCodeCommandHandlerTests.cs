using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Commands.GenerateEntityIdCode;
using TopLab.Application.Features.ExternalEntities.Common.Interfaces;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using Xunit;

namespace TopLab.Application.Tests.Features.ExternalEntities;

public class GenerateEntityIdCodeCommandHandlerTests
{
    private sealed class StubGenerator : IEntityIdCodeGenerator
    {
        private readonly Queue<string> _codes;

        public StubGenerator(params string[] codes)
        {
            _codes = new Queue<string>(codes);
        }

        public string Generate() => _codes.Count > 0 ? _codes.Dequeue() : "FALLBACK01";
    }

    private static FakeApplicationDbContext BuildDb()
    {
        var db = new FakeApplicationDbContext();
        db.Add(ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr. Ahmed"));
        return db;
    }

    [Fact]
    public async Task Generate_SetsNonEmptyUniqueCode()
    {
        var db = BuildDb();
        var handler = new GenerateEntityIdCodeCommandHandler(db, new StubGenerator("AB12CD34"));

        var result = await handler.Handle(new GenerateEntityIdCodeCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("AB12CD34", result.Value);
        Assert.Equal("AB12CD34", db.ExternalEntities[0].GeneratedIdCode);
    }

    [Fact]
    public async Task Generate_OverwritesPriorCode()
    {
        var db = BuildDb();
        var handler = new GenerateEntityIdCodeCommandHandler(db, new StubGenerator("FIRST", "SECOND"));
        await handler.Handle(new GenerateEntityIdCodeCommand(1), CancellationToken.None);

        var result = await handler.Handle(new GenerateEntityIdCodeCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("SECOND", result.Value);
    }

    [Fact]
    public async Task Generate_PreCheckCollision_RetriesWithNextCandidate()
    {
        var db = BuildDb();
        var other = ExternalEntity.Create(
            ExternalEntityId.Create(2), EntityType.TreatingDoctor, "Dr. Other");
        other.RegenerateIdCode("TAKEN01");
        db.Add(other);
        var handler = new GenerateEntityIdCodeCommandHandler(db, new StubGenerator("TAKEN01", "FRESH02"));

        var result = await handler.Handle(new GenerateEntityIdCodeCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("FRESH02", result.Value);
    }

    [Fact]
    public async Task Generate_SaveViolation_RetriesThenSucceeds()
    {
        var fake = new UniqueViolationFakeApplicationDbContext();
        fake.Inner.Add(ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr. Ahmed"));
        var throwing = new ThrowOnceFake(fake);
        var handler = new GenerateEntityIdCodeCommandHandler(throwing, new StubGenerator("C1", "C2"));

        var result = await handler.Handle(new GenerateEntityIdCodeCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("C2", result.Value);
    }

    [Fact]
    public async Task Generate_AlwaysColliding_ReturnsConflict()
    {
        var db = BuildDb();
        var handler = new GenerateEntityIdCodeCommandHandler(db, new StubGenerator("SAME", "SAME", "SAME", "SAME", "SAME", "SAME"));
        var other = ExternalEntity.Create(
            ExternalEntityId.Create(2), EntityType.TreatingDoctor, "Dr. Other");
        other.RegenerateIdCode("SAME");
        db.Add(other);

        var result = await handler.Handle(new GenerateEntityIdCodeCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("تعذر توليد رمز فريد، حاول مرة أخرى.", result.Error.Message);
    }

    [Fact]
    public async Task Generate_InvalidGeneratorOutput_ReturnsValidation()
    {
        var db = BuildDb();
        var handler = new GenerateEntityIdCodeCommandHandler(db, new StubGenerator("   "));

        var result = await handler.Handle(new GenerateEntityIdCodeCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal("رمز الجهة غير صالح.", result.Error.Message);
    }

    [Fact]
    public async Task Generate_Missing_ReturnsNotFound()
    {
        var handler = new GenerateEntityIdCodeCommandHandler(BuildDb(), new StubGenerator("X1"));

        var result = await handler.Handle(new GenerateEntityIdCodeCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    private sealed class ThrowOnceFake : TopLab.Application.Common.Interfaces.IApplicationDbContext
    {
        private readonly UniqueViolationFakeApplicationDbContext _inner;
        private bool _thrown;

        public ThrowOnceFake(UniqueViolationFakeApplicationDbContext inner)
        {
            _inner = inner;
        }

        public IQueryable<TEntity> Set<TEntity>() where TEntity : class => _inner.Set<TEntity>();

        public void Add<TEntity>(TEntity entity) where TEntity : class => _inner.Add(entity);

        public void Update<TEntity>(TEntity entity) where TEntity : class => _inner.Update(entity);

        public void Remove<TEntity>(TEntity entity) where TEntity : class => _inner.Remove(entity);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (!_thrown)
            {
                _thrown = true;
                throw new Exception("The INSERT statement conflicted with the UNIQUE INDEX 'IX_Test'. duplicate key value is (C1).");
            }

            return _inner.SaveChangesAsync(cancellationToken);
        }

        public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) =>
            _inner.CanConnectAsync(cancellationToken);
    }
}
