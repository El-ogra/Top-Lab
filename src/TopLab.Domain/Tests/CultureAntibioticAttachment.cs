using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

/// <summary>Composite PK: TestId + AntibioticId.</summary>
public sealed class CultureAntibioticAttachment
{
    public TestId TestId { get; private set; } = default!;

    public AntibioticId AntibioticId { get; private set; } = default!;

    private CultureAntibioticAttachment()
    {
    }

    public CultureAntibioticAttachment(TestId testId, AntibioticId antibioticId)
    {
        TestId = testId;
        AntibioticId = antibioticId;
    }
}
