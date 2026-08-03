using IdeasGroup.Kanban.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdeasGroup.Kanban.Infrastructure.Persistence.Configurations;

public class TaskLabelConfiguration : IEntityTypeConfiguration<TaskLabel>
{
    public void Configure(EntityTypeBuilder<TaskLabel> builder)
    {
        builder.ToTable("TaskLabels");

        builder.HasKey(tl => tl.Id);

        builder.HasOne<Label>()
            .WithMany()
            .HasForeignKey(tl => tl.LabelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(tl => new { tl.KanbanTaskId, tl.LabelId }).IsUnique();
    }
}
