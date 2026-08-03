using IdeasGroup.Kanban.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdeasGroup.Kanban.Infrastructure.Persistence.Configurations;

public class KanbanTaskConfiguration : IEntityTypeConfiguration<KanbanTask>
{
    public void Configure(EntityTypeBuilder<KanbanTask> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(4000);

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(t => new { t.BoardColumnId, t.Position });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.TaskLabels)
            .WithOne()
            .HasForeignKey(tl => tl.KanbanTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.TaskLabels).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
