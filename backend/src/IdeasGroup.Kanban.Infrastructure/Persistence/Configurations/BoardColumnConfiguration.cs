using IdeasGroup.Kanban.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdeasGroup.Kanban.Infrastructure.Persistence.Configurations;

public class BoardColumnConfiguration : IEntityTypeConfiguration<BoardColumn>
{
    public void Configure(EntityTypeBuilder<BoardColumn> builder)
    {
        builder.ToTable("BoardColumns");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(c => c.Tasks)
            .WithOne()
            .HasForeignKey(t => t.BoardColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Tasks).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
