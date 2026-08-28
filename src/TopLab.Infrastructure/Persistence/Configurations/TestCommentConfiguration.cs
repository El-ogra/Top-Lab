using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Tests;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class TestCommentConfiguration : IEntityTypeConfiguration<TestComment>
{
    public void Configure(EntityTypeBuilder<TestComment> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => TestCommentId.Create(v)).ValueGeneratedOnAdd().HasColumnName("TestCommentId");
        b.Property(e => e.TestId).HasConversion(v => v.Value, v => TestId.Create(v)).IsRequired();
        b.Property(e => e.CommentText).HasMaxLength(1000).IsRequired();
        b.HasOne<Test>().WithMany().HasForeignKey(e => e.TestId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(e => e.TestId);
    }
}
