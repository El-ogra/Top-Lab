using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class PrinterAssignmentConfiguration : IEntityTypeConfiguration<PrinterAssignment>
{
    public void Configure(EntityTypeBuilder<PrinterAssignment> b)
    {
        b.HasKey(e => e.OutputType);
        b.Property(e => e.OutputType).HasConversion<int>().HasColumnType("tinyint");
        b.Property(e => e.PrinterName).HasMaxLength(200).IsRequired();
        b.HasData(
            new PrinterAssignment(TopLab.Domain.Common.Enums.PrinterOutputType.Reports, "Reports"),
            new PrinterAssignment(TopLab.Domain.Common.Enums.PrinterOutputType.Barcode, "Barcode"),
            new PrinterAssignment(TopLab.Domain.Common.Enums.PrinterOutputType.Envelope, "Envelope"),
            new PrinterAssignment(TopLab.Domain.Common.Enums.PrinterOutputType.Receipt, "Receipt")
        );
    }
}
