using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PatientRegistration.Commands.AddProfileToVisit;

/// <summary>
/// Typed profile ordering (Decision 2): the stored charge is exactly
/// <see cref="PatientAccountCalculator.ProfileSelectionCharge"/> of the profile's
/// immutable FixedPrice. Constituent analytes/tests, price lists and custom groups
/// never influence this price.
/// </summary>
public sealed class AddProfileToVisitCommandHandler : IRequestHandler<AddProfileToVisitCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public AddProfileToVisitCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(AddProfileToVisitCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null)
        {
            return Result<int>.Failure(Error.NotFound("المريض غير موجود."));
        }

        if (patient.IsDeleted)
        {
            return Result<int>.Failure(Error.Conflict("المريض محذوف ولا يمكن إضافة بروفايل إليه."));
        }

        var profile = _db.Set<Profile>().FirstOrDefault(p => p.Id.Value == request.ProfileId);
        if (profile is null)
        {
            return Result<int>.Failure(Error.NotFound("البروفايل غير موجود."));
        }

        if (!profile.IsActive)
        {
            return Result<int>.Failure(Error.Conflict("البروفايل غير مفعّل."));
        }

        var test = _db.Set<Test>().FirstOrDefault(t => t.Id.Equals(profile.TestId));
        if (test is null || !test.IsActive || test.ResultKind != ResultKind.SpecializedProfile)
        {
            return Result<int>.Failure(Error.Conflict("تحليل البروفايل غير صالح."));
        }

        var price = PatientAccountCalculator.ProfileSelectionCharge(profile.FixedPrice);

        var pt = PatientTest.Create(
            PatientTestId.Create(0),
            patient.Id,
            profile.TestId,
            price,
            request.IsUrine,
            request.IsStool,
            request.IsBlood,
            request.IsSemen,
            request.IsCsf,
            request.IsTakenOutsideLab);

        _db.Add(pt);

        await _db.SaveChangesAsync(cancellationToken);

        var newId = _db.Set<PatientTest>()
            .Where(x => x.PatientId.Equals(patient.Id))
            .OrderByDescending(x => x.Id.Value)
            .Select(x => x.Id.Value)
            .FirstOrDefault();

        return Result<int>.Success(newId);
    }
}