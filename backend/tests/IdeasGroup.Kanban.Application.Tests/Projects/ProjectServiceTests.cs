using IdeasGroup.Kanban.Application.Projects;
using IdeasGroup.Kanban.Application.Tests.Auth;
using IdeasGroup.Kanban.Domain.Entities;
using IdeasGroup.Kanban.Domain.Exceptions;
using Xunit;

namespace IdeasGroup.Kanban.Application.Tests.Projects;

public class ProjectServiceTests
{
    private static User CreateUser(string email = "owner@kanban.local") =>
        User.Create("Usuario de Prueba", email, "hash");

    [Fact]
    public async Task CreateAsync_crea_proyecto_con_propietario_y_tablero_con_columnas_por_defecto()
    {
        var repository = new FakeProjectRepository();
        var service = new ProjectService(repository, new FakeUserRepository());
        var ownerId = Guid.NewGuid();

        var response = await service.CreateAsync(ownerId, new CreateProjectRequest("Proyecto A", "Descripción"));

        Assert.Equal(ownerId, response.OwnerId);
        Assert.Single(response.Members);
        Assert.Equal(ownerId, response.Members[0].UserId);
        var board = Assert.Single(repository.Boards);
        Assert.Equal(3, board.Columns.Count);
    }

    [Fact]
    public async Task ListForUserAsync_solo_retorna_proyectos_donde_el_usuario_es_miembro()
    {
        var repository = new FakeProjectRepository();
        var ownerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var project = Project.Create("Proyecto A", null, ownerId);
        repository.Seed(project);
        var service = new ProjectService(repository, new FakeUserRepository());

        var mine = await service.ListForUserAsync(ownerId);
        var others = await service.ListForUserAsync(otherId);

        Assert.Single(mine.Items);
        Assert.Equal(1, mine.TotalCount);
        Assert.Empty(others.Items);
        Assert.Equal(0, others.TotalCount);
    }

    [Fact]
    public async Task ListForUserAsync_pagina_y_filtra_por_nombre_con_coincidencia_parcial()
    {
        var repository = new FakeProjectRepository();
        var ownerId = Guid.NewGuid();
        repository.Seed(Project.Create("Rediseño Web", null, ownerId));
        repository.Seed(Project.Create("App Móvil", null, ownerId));
        repository.Seed(Project.Create("Rediseño App", null, ownerId));
        var service = new ProjectService(repository, new FakeUserRepository());

        var filtered = await service.ListForUserAsync(ownerId, page: 1, pageSize: 10, search: "rediseño");
        var firstPage = await service.ListForUserAsync(ownerId, page: 1, pageSize: 2, search: null);
        var secondPage = await service.ListForUserAsync(ownerId, page: 2, pageSize: 2, search: null);

        Assert.Equal(2, filtered.TotalCount);
        Assert.All(filtered.Items, p => Assert.Contains("rediseño", p.Name, StringComparison.OrdinalIgnoreCase));

        Assert.Equal(3, firstPage.TotalCount);
        Assert.Equal(2, firstPage.Items.Count);
        Assert.Single(secondPage.Items);
    }

    [Fact]
    public async Task GetByIdAsync_lanza_ForbiddenProjectAccessException_si_el_usuario_no_es_miembro()
    {
        var repository = new FakeProjectRepository();
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Proyecto A", null, ownerId);
        repository.Seed(project);
        var service = new ProjectService(repository, new FakeUserRepository());

        await Assert.ThrowsAsync<ForbiddenProjectAccessException>(
            () => service.GetByIdAsync(Guid.NewGuid(), project.Id));
    }

    [Fact]
    public async Task GetByIdAsync_lanza_ProjectNotFoundException_si_el_proyecto_no_existe()
    {
        var service = new ProjectService(new FakeProjectRepository(), new FakeUserRepository());

        await Assert.ThrowsAsync<ProjectNotFoundException>(
            () => service.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_lanza_ForbiddenProjectAccessException_si_quien_edita_no_es_el_propietario()
    {
        var repository = new FakeProjectRepository();
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Proyecto A", null, ownerId);
        var memberId = Guid.NewGuid();
        project.AddMember(memberId, Domain.Enums.ProjectRole.Member);
        repository.Seed(project);
        var service = new ProjectService(repository, new FakeUserRepository());

        await Assert.ThrowsAsync<ForbiddenProjectAccessException>(
            () => service.UpdateAsync(memberId, project.Id, new UpdateProjectRequest("Nuevo nombre", null, null, null, Domain.Enums.ProjectStatus.Planned)));
    }

    [Fact]
    public async Task DeleteAsync_permite_borrar_solo_al_propietario()
    {
        var repository = new FakeProjectRepository();
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Proyecto A", null, ownerId);
        repository.Seed(project);
        var service = new ProjectService(repository, new FakeUserRepository());

        await service.DeleteAsync(ownerId, project.Id);

        await Assert.ThrowsAsync<ProjectNotFoundException>(() => service.GetByIdAsync(ownerId, project.Id));
    }

    [Fact]
    public async Task AddMemberAsync_agrega_un_usuario_existente_por_email()
    {
        var repository = new FakeProjectRepository();
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Proyecto A", null, ownerId);
        repository.Seed(project);
        var member = CreateUser("nuevo@kanban.local");
        var userRepository = new FakeUserRepository().Seed(member);
        var service = new ProjectService(repository, userRepository);

        var response = await service.AddMemberAsync(ownerId, project.Id, new AddMemberRequest("nuevo@kanban.local"));

        Assert.Equal(member.Id, response.UserId);
        Assert.Equal("Member", response.Role);
    }

    [Fact]
    public async Task AddMemberAsync_lanza_DomainException_si_el_email_no_existe()
    {
        var repository = new FakeProjectRepository();
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Proyecto A", null, ownerId);
        repository.Seed(project);
        var service = new ProjectService(repository, new FakeUserRepository());

        await Assert.ThrowsAsync<DomainException>(
            () => service.AddMemberAsync(ownerId, project.Id, new AddMemberRequest("inexistente@kanban.local")));
    }

    [Fact]
    public async Task RemoveMemberAsync_no_permite_quitar_al_propietario()
    {
        var repository = new FakeProjectRepository();
        var ownerId = Guid.NewGuid();
        var project = Project.Create("Proyecto A", null, ownerId);
        repository.Seed(project);
        var service = new ProjectService(repository, new FakeUserRepository());

        await Assert.ThrowsAsync<DomainException>(
            () => service.RemoveMemberAsync(ownerId, project.Id, ownerId));
    }
}
