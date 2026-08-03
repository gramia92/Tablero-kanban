using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdeasGroup.Kanban.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(KanbanDbContext dbContext, IPasswordHasher passwordHasher)
    {
        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        var admin = User.Create("Administrador", "admin@kanban.local", passwordHasher.Hash("Admin123!"));
        var member = User.Create("Usuario Demo", "demo@kanban.local", passwordHasher.Hash("Demo123!"));

        dbContext.Users.AddRange(admin, member);

        await dbContext.SaveChangesAsync();
    }
}
