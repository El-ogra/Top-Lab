using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Application.Features.ExternalEntities.Common.Interfaces;
using TopLab.Domain.ExternalEntities;

namespace TopLab.Application.Features.ExternalEntities.Commands.GenerateEntityIdCode;

public sealed class GenerateEntityIdCodeCommandHandler : IRequestHandler<GenerateEntityIdCodeCommand, Result<string>>
{
    private const int MaxAttempts = 5;

    private readonly IApplicationDbContext _db;
    private readonly IEntityIdCodeGenerator _generator;

    public GenerateEntityIdCodeCommandHandler(IApplicationDbContext db, IEntityIdCodeGenerator generator)
    {
        _db = db;
        _generator = generator;
    }

    public async Task<Result<string>> Handle(GenerateEntityIdCodeCommand request, CancellationToken cancellationToken)
    {
        var entity = _db.Set<ExternalEntity>().FirstOrDefault(e => e.Id.Value == request.Id);
        if (entity is null)
        {
            return Result<string>.Failure(Error.NotFound("الجهة الخارجية غير موجودة."));
        }

        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var candidate = _generator.Generate();
            if (_db.Set<ExternalEntity>().Any(e => e.Id.Value != request.Id && e.GeneratedIdCode == candidate))
            {
                continue;
            }

            try
            {
                entity.RegenerateIdCode(candidate);
            }
            catch (ArgumentException)
            {
                return Result<string>.Failure(Error.Validation("رمز الجهة غير صالح."));
            }

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex) when (IsUniqueViolation(ex))
            {
                continue;
            }

            return Result<string>.Success(candidate.Trim());
        }

        return Result<string>.Failure(Error.Conflict("تعذر توليد رمز فريد، حاول مرة أخرى."));
    }

    private static bool IsUniqueViolation(Exception ex)
    {
        var msg = ex.Message;
        return msg.Contains("UNIQUE") || msg.Contains("duplicate");
    }
}
