using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => PermissionId.Create(v)).ValueGeneratedOnAdd();
        b.Property(e => e.Code).HasMaxLength(50).IsRequired();
        b.HasIndex(e => e.Code).IsUnique();
        b.Property(e => e.Description).HasMaxLength(300).IsRequired();
        b.HasData(
            new { Id = PermissionId.Create(1), Code = "ADD_EDIT_PATIENT", Description = "Add and edit patient data" },
            new { Id = PermissionId.Create(2), Code = "EDIT_RESULTS", Description = "Enter and edit patient test results" },
            new { Id = PermissionId.Create(3), Code = "REVIEW_RESULTS", Description = "Review and edit patient test results" },
            new { Id = PermissionId.Create(4), Code = "PRINT_RESULTS", Description = "Print patient results" },
            new { Id = PermissionId.Create(5), Code = "BLOCK_PRINT_ON_BALANCE", Description = "Block printing when balance remains" },
            new { Id = PermissionId.Create(6), Code = "DELIVER_RESULTS", Description = "Deliver results" },
            new { Id = PermissionId.Create(7), Code = "DISCOUNT_LIMIT", Description = "Discount limit" },
            new { Id = PermissionId.Create(8), Code = "PRINT_WORKSHEET", Description = "Print worksheet and Log" },
            new { Id = PermissionId.Create(9), Code = "DELETE_PATIENT", Description = "Delete patients" },
            new { Id = PermissionId.Create(10), Code = "EDIT_SYSTEM_SETTINGS", Description = "Edit system and test settings" },
            new { Id = PermissionId.Create(11), Code = "CASH_DISBURSE_DEPOSIT", Description = "Cash disbursement and deposit" },
            new { Id = PermissionId.Create(12), Code = "STATISTICS", Description = "Statistics" },
            new { Id = PermissionId.Create(13), Code = "PT_AUDIT_ACCESS", Description = "P/T audit access" }
        );
    }
}
