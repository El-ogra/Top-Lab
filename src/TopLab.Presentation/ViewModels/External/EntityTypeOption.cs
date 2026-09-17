using TopLab.Domain.Common.Enums;

namespace TopLab.Presentation.ViewModels.External;

public sealed class EntityTypeOption
{
    public EntityTypeOption(EntityType? value, string label)
    {
        Value = value;
        Label = label;
    }

    public EntityType? Value { get; }
    public string Label { get; }

    public static string LabelFor(EntityType type) => type switch
    {
        EntityType.TreatingDoctor => "طبيب معالج",
        EntityType.ReferralOrContract => "جهة إحالة/تعاقد",
        EntityType.PartnerLab => "معمل شريك",
        _ => type.ToString()
    };
}
