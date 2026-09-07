using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.PatientRegistration.Queries.GetRegistrationCatalog;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Queries;

public class GetRegistrationCatalogQueryHandlerTests
{
    private static (FakeApplicationDbContext Db, ISender Sender) BuildHost()
    {
        var db = new FakeApplicationDbContext();

        var services = new ServiceCollection();
        services.AddSingleton<IApplicationDbContext>(db);
        services.AddSingleton<ICurrentUserService>(new FakeCurrentUserService { IsAbsolutePermission = true });
        services.AddSingleton<IAppLogger>(new FakeAppLogger());
        services.AddApplication();
        var provider = services.BuildServiceProvider();

        return (db, provider.GetRequiredService<ISender>());
    }

    [Fact]
    public async Task Handle_AggregatesAllSources_AndReadsSystemSettings()
    {
        var (db, sender) = BuildHost();

        db.PatientTitles.Add(PatientTitle.Create(PatientTitleId.Create(1), "Mr."));
        db.MedicalConditionTypes.Add(MedicalConditionType.Create(MedicalConditionTypeId.Create(1), "Diabetes", MedicalConditionCategory.Condition));
        db.SystemSettings.Add(SystemSettings.CreateDefault());

        var result = await sender.Send(new GetRegistrationCatalogQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AccountType.Individual, result.Value!.DefaultAccountType);
        Assert.False(result.Value.DisableAutoTitleInsertion);
        Assert.Single(result.Value.PatientTitles);
        Assert.Single(result.Value.MedicalConditionTypes);
    }

    [Fact]
    public async Task Handle_ResolvesReferralPlaceholders_BySex()
    {
        var (db, sender) = BuildHost();

        db.SystemSettings.Add(SystemSettings.CreateDefault());

        var result = await sender.Send(new GetRegistrationCatalogQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Himself", result.Value!.ReferralPlaceholderMale);
        Assert.Equal("Herself", result.Value.ReferralPlaceholderFemale);
    }

    [Fact]
    public async Task Handle_NoSystemSettings_Fails()
    {
        var (_, sender) = BuildHost();

        var result = await sender.Send(new GetRegistrationCatalogQuery(), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}