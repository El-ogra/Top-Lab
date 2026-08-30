namespace TopLab.Domain.Common.Ids;

public sealed class LabId : StronglyTypedId<string>
{
    private LabId(string value) : base(value) { }
    public static LabId Create(string value) => new(value ?? throw new ArgumentNullException(nameof(value)));
}