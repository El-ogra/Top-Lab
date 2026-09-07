using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureAndAntibiotics.Common;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.CultureAndAntibiotics.Commands.CreateAntibiotic;

public sealed class CreateAntibioticCommandHandler
    : IRequestHandler<CreateAntibioticCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public CreateAntibioticCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(
        CreateAntibioticCommand request, CancellationToken cancellationToken)
    {
        var trimmed = request.Name.Trim();

        if (_db.Set<Antibiotic>().Any(a => a.Name == trimmed))
        {
            return Result<int>.Failure(Error.Conflict("المضاد الحيوي موجود بالفعل"));
        }

        Antibiotic antibiotic;
        try
        {
            antibiotic = Antibiotic.Create(
                AntibioticId.Create(0),
                trimmed,
                request.IsPregnancyFlagged,
                request.IsChildrenFlagged);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        _db.Add(antibiotic);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(antibiotic.Id.Value);
    }
}