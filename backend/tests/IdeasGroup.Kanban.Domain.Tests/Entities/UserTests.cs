using IdeasGroup.Kanban.Domain.Entities;
using IdeasGroup.Kanban.Domain.Exceptions;

namespace IdeasGroup.Kanban.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_NormalizesEmailAndTrimsName()
    {
        var user = User.Create("  Ada Lovelace  ", "  ADA@Example.com ", "hash");

        Assert.Equal("Ada Lovelace", user.FullName);
        Assert.Equal("ada@example.com", user.Email);
    }

    [Theory]
    [InlineData("", "a@b.com", "hash")]
    [InlineData("Name", "", "hash")]
    [InlineData("Name", "a@b.com", "")]
    public void Create_WithMissingRequiredField_ThrowsDomainException(string fullName, string email, string passwordHash)
    {
        Assert.Throws<DomainException>(() => User.Create(fullName, email, passwordHash));
    }
}
