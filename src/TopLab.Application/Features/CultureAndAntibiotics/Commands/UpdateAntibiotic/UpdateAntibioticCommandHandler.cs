using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureAndAntibiotics.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.CultureAndAntibiotics.Commands.UpdateAntibiotic;

public sealed class UpdateAntibioticCommandHandler
    : IRequestHandler<UpdateAntibioticCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateAntibioticCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(
        UpdateAntibioticCommand request, CancellationToken cancellationToken)
    {
        var antibiotic = _db.Set<Antibiotic>().FirstOrDefault(a => a.Id.Value == request.Id);
        if (antibiotic is null)
        {
            return Result.Failure(Error.NotFound("المضاد الحيوي غير موجود."));
        }

        var trimmed = request.Name.Trim();

        if (_db.Set<Antibiotic>().Any(a => a.Id.Value != request.Id && a.Name == trimmed))
        {
            return Result.Failure(Error.Conflict("المضاد الحيوي موجود بالفعل"));
        }

        try
        {
            antibiotic.Update(trimmed, request.IsPregnancyFlagged, request.IsChildrenFlagged);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}