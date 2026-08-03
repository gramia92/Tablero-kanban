using IdeasGroup.Kanban.Domain.Common;
using IdeasGroup.Kanban.Domain.Exceptions;

namespace IdeasGroup.Kanban.Domain.Entities;

public class User : AuditableEntity
{
    public string FullName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;

    private User()
    {
    }

    public static User Create(string fullName, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainException("El nombre del usuario es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("El email del usuario es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("El password hash es obligatorio.");
        }

        return new User
        {
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash
        };
    }

    public void UpdateProfile(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainException("El nombre del usuario es obligatorio.");
        }

        FullName = fullName.Trim();
        Touch();
    }

    public void ChangePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("El password hash es obligatorio.");
        }

        PasswordHash = passwordHash;
        Touch();
    }
}
