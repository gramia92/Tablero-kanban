using IdeasGroup.Kanban.Application.Abstractions;
using IdeasGroup.Kanban.Domain.Entities;
using IdeasGroup.Kanban.Domain.Exceptions;

namespace IdeasGroup.Kanban.Application.Auth;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        return BuildResponse(user);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new EmailAlreadyRegisteredException();
        }

        var user = User.Create(request.FullName, request.Email, _passwordHasher.Hash(request.Password));
        await _userRepository.AddAsync(user, cancellationToken);

        return BuildResponse(user);
    }

    private AuthResponse BuildResponse(User user)
    {
        var token = _jwtTokenGenerator.Generate(user);
        return new AuthResponse(token.AccessToken, token.ExpiresAtUtc, user.Id, user.FullName, user.Email);
    }
}
