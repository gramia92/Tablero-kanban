namespace IdeasGroup.Kanban.Application.Auth;

public record AuthResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string FullName,
    string Email);
