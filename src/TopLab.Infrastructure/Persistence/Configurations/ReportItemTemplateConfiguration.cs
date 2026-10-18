using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class ReportItemTemplateConfiguration : IEntityTypeConfiguration<ReportItemTemplate>
{
    public void Configure(EntityTypeBuilder<ReportItemTemplate> b)
    {
        b.HasKey(e => e.ItemKey);
        b.Property(e => e.ItemKey).HasMaxLength(ReportItemTemplate.MaxItemKeyLength).IsRequired();
        b.Property(e => e.Label).HasMaxLength(ReportItemTemplate.MaxLabelLength).IsRequired();
        b.Property(e => e.ReferenceValueExpression).HasMaxLength(ReportItemTemplate.MaxReferenceValueExpressionLength).IsRequired(false);
        b.Property(e => e.ShowHide).IsRequired().HasDefaultValue(true);
        b.Property(e => e.ListMembership).HasMaxLength(ReportItemTemplate.MaxListMembershipLength).IsRequired(false);
        b.Property(e => e.FreeTextOverride).IsRequired().HasDefaultValue(false);
        b.HasData(ReportItemTemplateCatalog.Defaults.ToArray());
    }
}
