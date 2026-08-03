using IdeasGroup.Kanban.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdeasGroup.Kanban.Infrastructure.Persistence.Configurations;

public class LabelConfiguration : IEntityTypeConfiguration<Label>
{
    public void Configure(EntityTypeBuilder<Label> builder)
    {
        builder.ToTable("Labels");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Color)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(l => new { l.ProjectId, l.Name }).IsUnique();
    }
}
