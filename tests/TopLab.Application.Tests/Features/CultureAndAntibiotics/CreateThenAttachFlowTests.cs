using TopLab.Application.Features.CultureAndAntibiotics.Commands.AttachAntibioticToCulture;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.CreateAntibiotic;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.CultureAndAntibiotics;

/// <summary>
/// Verifies the confirmed D6-a two-command manual-entry flow: caller creates the
/// antibiotic first, then attaches it to the culture. No composite command exists.
/// </summary>
public class CreateThenAttachFlowTests
{
    private static Test BuildCultureTest(int id)
        => Test.Create(
            TestId.Create(id),
            "Blood Culture",
            reportName: "Blood Culture",
            receiptName: "Blood Culture",
            testCode: $"C{id}",
            completionDurationMinutes: 30,
            patientPrice: 100m,
            resultKind: ResultKind.Culture,
            isCultureType: true);

    [Fact]
    public async Task CreateAntibiotic_ThenAttach_ToCulture_BothSucceed()
    {
        var db = new FakeApplicationDbContext();
        db.Add(BuildCultureTest(1));
        db.Add(Antibiotic.Create(AntibioticId.Create(7), "Pre-existing"));

        var create = new CreateAntibioticCommandHandler(db);
        var createResult = await create.Handle(
            new CreateAntibioticCommand("  ManuallyEntered  ", false, true),
            CancellationToken.None);

        Assert.True(createResult.IsSuccess);
        Assert.Contains(db.Antibiotics, a => a.Name == "ManuallyEntered");

        var attach = new AttachAntibioticToCultureCommandHandler(db);
        var attachResult = await attach.Handle(
            new AttachAntibioticToCultureCommand(1, createResult.Value),
            CancellationToken.None);

        Assert.True(attachResult.IsSuccess);
        Assert.Single(db.CultureAntibioticAttachments,
            a => a.TestId.Value == 1
                 && a.AntibioticId.Value == createResult.Value);
    }

    [Fact]
    public async Task CreateAntibiotic_Standalone_LeavesNoAttachment()
    {
        var db = new FakeApplicationDbContext();
        db.Add(BuildCultureTest(1));

        var create = new CreateAntibioticCommandHandler(db);
        var createResult = await create.Handle(
            new CreateAntibioticCommand("Standalone", false, false),
            CancellationToken.None);

        Assert.True(createResult.IsSuccess);
        Assert.Empty(db.CultureAntibioticAttachments);
    }
}