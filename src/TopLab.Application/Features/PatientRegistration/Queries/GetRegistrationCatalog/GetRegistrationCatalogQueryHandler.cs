using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Features.PatientRegistration.Queries.GetMedicalConditionTypes;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientTitles;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestGroups;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.PatientRegistration.Queries.GetRegistrationCatalog;

public sealed class GetRegistrationCatalogQueryHandler
    : IRequestHandler<GetRegistrationCatalogQuery, Result<RegistrationCatalogDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ISender _sender;

    public GetRegistrationCatalogQueryHandler(IApplicationDbContext db, ISender sender)
    {
        _db = db;
        _sender = sender;
    }

    public async Task<Result<RegistrationCatalogDto>> Handle(
        GetRegistrationCatalogQuery request, CancellationToken cancellationToken)
    {
        var testsResult = await _sender.Send(new SearchTestCatalogQuery(null, null), cancellationToken);
        if (!testsResult.IsSuccess)
        {
            return Result<RegistrationCatalogDto>.Failure(testsResult.Error!);
        }

        var groupsResult = await _sender.Send(new GetTestGroupsQuery(), cancellationToken);
        if (!groupsResult.IsSuccess)
        {
            return Result<RegistrationCatalogDto>.Failure(groupsResult.Error!);
        }

        var titlesResult = await _sender.Send(new GetPatientTitlesQuery(), cancellationToken);
        if (!titlesResult.IsSuccess)
        {
            return Result<RegistrationCatalogDto>.Failure(titlesResult.Error!);
        }

        var conditionsResult = await _sender.Send(new GetMedicalConditionTypesQuery(), cancellationToken);
        if (!conditionsResult.IsSuccess)
        {
            return Result<RegistrationCatalogDto>.Failure(conditionsResult.Error!);
        }

        var settingsResult = await _sender.Send(new GetSystemSettingsQuery(), cancellationToken);
        if (!settingsResult.IsSuccess)
        {
            return Result<RegistrationCatalogDto>.Failure(settingsResult.Error!);
        }

        var settings = settingsResult.Value!;

        var malePlaceholder = ReferralNameResolver.Resolve(null, Sex.Male);
        var femalePlaceholder = ReferralNameResolver.Resolve(null, Sex.Female);

        var dto = new RegistrationCatalogDto(
            testsResult.Value!,
            groupsResult.Value!,
            titlesResult.Value!,
            conditionsResult.Value!,
            settings.DefaultAccountType,
            settings.DisableAutoTitleInsertion,
            malePlaceholder,
            femalePlaceholder);

        return Result<RegistrationCatalogDto>.Success(dto);
    }
}