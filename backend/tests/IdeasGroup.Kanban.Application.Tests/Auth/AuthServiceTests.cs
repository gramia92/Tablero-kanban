using IdeasGroup.Kanban.Application.Auth;
using IdeasGroup.Kanban.Domain.Entities;
using IdeasGroup.Kanban.Domain.Exceptions;

namespace IdeasGroup.Kanban.Application.Tests.Auth;

public class AuthServiceTests
{
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeJwtTokenGenerator _jwtTokenGenerator = new();

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var user = User.Create("Usuario Demo", "demo@kanban.local", _passwordHasher.Hash("Demo123!"));
        var repository = new FakeUserRepository().Seed(user);
        var sut = new AuthService(repository, _passwordHasher, _jwtTokenGenerator);

        var result = await sut.LoginAsync(new LoginRequest("demo@kanban.local", "Demo123!"));

        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("fake-token-for-demo@kanban.local", result.AccessToken);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsInvalidCredentials()
    {
        var user = User.Create("Usuario Demo", "demo@kanban.local", _passwordHasher.Hash("Demo123!"));
        var repository = new FakeUserRepository().Seed(user);
        var sut = new AuthService(repository, _passwordHasher, _jwtTokenGenerator);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => sut.LoginAsync(new LoginRequest("demo@kanban.local", "wrong-password")));
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ThrowsInvalidCredentials()
    {
        var repository = new FakeUserRepository();
        var sut = new AuthService(repository, _passwordHasher, _jwtTokenGenerator);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => sut.LoginAsync(new LoginRequest("nadie@kanban.local", "Demo123!")));
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_CreatesUserAndReturnsToken()
    {
        var repository = new FakeUserRepository();
        var sut = new AuthService(repository, _passwordHasher, _jwtTokenGenerator);

        var result = await sut.RegisterAsync(new RegisterRequest("Nuevo Usuario", "nuevo@kanban.local", "Nuevo123!"));

        Assert.Equal("nuevo@kanban.local", result.Email);
        Assert.True(await repository.EmailExistsAsync("nuevo@kanban.local"));
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsEmailAlreadyRegistered()
    {
        var existing = User.Create("Usuario Demo", "demo@kanban.local", _passwordHasher.Hash("Demo123!"));
        var repository = new FakeUserRepository().Seed(existing);
        var sut = new AuthService(repository, _passwordHasher, _jwtTokenGenerator);

        await Assert.ThrowsAsync<EmailAlreadyRegisteredException>(
            () => sut.RegisterAsync(new RegisterRequest("Otro", "demo@kanban.local", "Otro123!")));
    }
}
