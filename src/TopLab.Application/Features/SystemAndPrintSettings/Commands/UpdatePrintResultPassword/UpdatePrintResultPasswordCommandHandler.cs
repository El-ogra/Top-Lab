using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdatePrintResultPassword;

public sealed class UpdatePrintResultPasswordCommandHandler
    : IRequestHandler<UpdatePrintResultPasswordCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdatePrintResultPasswordCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(UpdatePrintResultPasswordCommand request, CancellationToken cancellationToken)
    {
        var settings = _db.Set<SystemSettings>().SingleOrDefault(s => s.Id == 1);
        if (settings is null)
        {
            return Result.Failure(Error.Unexpected("سجل الإعدادات العامة مفقود."));
        }

        string? hash = null;

        if (request.Enabled)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Result.Failure(Error.Validation("كلمة مرور الطباعة مطلوبة."));
            }

            hash = PrintResultPasswordHasher.Hash(request.Password);
        }

        settings.SetPrintResultPassword(request.Enabled, hash);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
