using TopLab.Application.Features.CultureAndAntibiotics.Commands.CreateAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Commands.UpdateAntibiotic;
using TopLab.Application.Features.CultureAndAntibiotics.Queries.GetAntibiotics;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.CultureAndAntibiotics;

/// <summary>W-02 S10 (WP-14): symbol + scientific name round-trip; nothing commercial.</summary>
public class AntibioticMasterFieldsTests
{
    [Fact]
    public async Task Antibiotic_Create_PersistsSymbolAndScientificName()
    {
        var db = new FakeApplicationDbContext();

        var result = await new CreateAntibioticCommandHandler(db).Handle(
            new CreateAntibioticCommand("Amoxicillin", false, false, "AMX", "Amoxicillin trihydrate"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(db.Set<TopLab.Domain.Tests.Antibiotic>());
        Assert.Equal("AMX", row.Symbol);
        Assert.Equal("Amoxicillin trihydrate", row.ScientificName);
    }

    [Fact]
    public async Task Antibiotic_Update_RoundTripsBoth()
    {
        var db = new FakeApplicationDbContext();
        var created = await new CreateAntibioticCommandHandler(db).Handle(
            new CreateAntibioticCommand("Amoxicillin", false, false),
            CancellationToken.None);
        Assert.True(created.IsSuccess);
        var id = created.Value;

        var updated = await new UpdateAntibioticCommandHandler(db).Handle(
            new UpdateAntibioticCommand(id, "Amoxicillin", true, false, "AMX", "Amoxicillin trihydrate"),
            CancellationToken.None);

        Assert.True(updated.IsSuccess);
        var row = Assert.Single(db.Set<TopLab.Domain.Tests.Antibiotic>());
        Assert.Equal("AMX", row.Symbol);
        Assert.Equal("Amoxicillin trihydrate", row.ScientificName);
        Assert.True(row.IsPregnancyFlagged);
    }

    [Fact]
    public async Task Antibiotic_BothOptional_DefaultsToNull()
    {
        var db = new FakeApplicationDbContext();

        var result = await new CreateAntibioticCommandHandler(db).Handle(
            new CreateAntibioticCommand("Penicillin", false, false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(db.Set<TopLab.Domain.Tests.Antibiotic>());
        Assert.Null(row.Symbol);
        Assert.Null(row.ScientificName);
    }

    [Fact]
    public async Task Antibiotic_RejectsSymbolOverTen()
    {
        var result = await new CreateAntibioticCommandValidator().ValidateAsync(
            new CreateAntibioticCommand("Amoxicillin", false, false, new string('x', 11), null));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Antibiotic_RejectsScientificNameOver150()
    {
        var result = await new CreateAntibioticCommandValidator().ValidateAsync(
            new CreateAntibioticCommand("Amoxicillin", false, false, null, new string('x', 151)));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task GetAntibiotics_ReturnsSymbolAndScientificName()
    {
        var db = new FakeApplicationDbContext();
        await new CreateAntibioticCommandHandler(db).Handle(
            new CreateAntibioticCommand("Amoxicillin", false, false, "AMX", "Amoxicillin trihydrate"),
            CancellationToken.None);

        var result = await new GetAntibioticsQueryHandler(db).Handle(
            new GetAntibioticsQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!);
        Assert.Equal("AMX", dto.Symbol);
        Assert.Equal("Amoxicillin trihydrate", dto.ScientificName);
    }
}
