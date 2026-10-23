using Microsoft.AspNetCore.Identity;
using UniversityApi.Common;
using UniversityApi.DTO.Auth;
using UniversityApi.Models;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly ITokenService _tokens;
    private readonly ILogger<AuthService> _logger;

    public AuthService(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn,
        ITokenService tokens, ILogger<AuthService> logger)
    {
        _users = users;
        _signIn = signIn;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task<ReturnResult<UserDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        var user = new ApplicationUser { UserName = dto.UserName, Email = dto.Email };

        // Хеширование пароля выполняет Identity (IPasswordHasher), собственной реализации нет
        var created = await _users.CreateAsync(user, dto.Password);
        if (!created.Succeeded)
        {
            if (created.Errors.Any(e => e.Code is "DuplicateUserName" or "DuplicateEmail"))
            {
                _logger.LogWarning("Registration rejected: user name or email already exists");
                return ReturnResult<UserDto>.Fail(409, ErrorCodes.UserNameExists,
                    "User name or email is already taken");
            }

            var errors = new Dictionary<string, string[]>
            {
                ["identity"] = created.Errors.Select(e => e.Description).ToArray()
            };
            return ReturnResult<UserDto>.Fail(400, ErrorCodes.RegistrationFailed, "Registration failed", errors);
        }

        // Регистрация НИКОГДА не даёт Admin: новый пользователь получает только роль Student (без привязки к записи Student)
        await _users.AddToRoleAsync(user, Roles.Student);

        _logger.LogInformation("User {UserId} registered with role {Role}", user.Id, Roles.Student);
        return ReturnResult<UserDto>.Created(await ToDtoAsync(user));
    }

    public async Task<ReturnResult<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _users.FindByNameAsync(dto.UserName);
        if (user is null)
        {
            _logger.LogWarning("Login failed: unknown user name");
            return InvalidCredentials();
        }

        // lockoutOnFailure: true — счётчик неудачных попыток и блокировка
        var result = await _signIn.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Login rejected: user {UserId} is locked out", user.Id);
            return ReturnResult<AuthResponseDto>.Fail(401, ErrorCodes.AccountLocked,
                "Account is temporarily locked because of repeated failed sign-in attempts");
        }

        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed for user {UserId}: wrong password", user.Id);
            return InvalidCredentials();
        }

        var roles = (await _users.GetRolesAsync(user)).ToList();
        var (token, expires) = _tokens.CreateToken(user, roles);

        _logger.LogInformation("User {UserId} signed in", user.Id);
        return ReturnResult<AuthResponseDto>.Ok(new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAtUtc = expires,
            UserName = user.UserName ?? string.Empty,
            Roles = roles
        });
    }

    public async Task<ReturnResult<UserDto>> GetCurrentAsync(string userId, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null)
            return ReturnResult<UserDto>.Fail(404, ErrorCodes.UserNotFound, "User not found");

        return ReturnResult<UserDto>.Ok(await ToDtoAsync(user));
    }

    private static ReturnResult<AuthResponseDto> InvalidCredentials()
        => ReturnResult<AuthResponseDto>.Fail(401, ErrorCodes.InvalidCredentials, "Invalid user name or password");

    private async Task<UserDto> ToDtoAsync(ApplicationUser user) => new()
    {
        Id = user.Id,
        UserName = user.UserName ?? string.Empty,
        Email = user.Email,
        Roles = (await _users.GetRolesAsync(user)).ToList(),
        StudentId = user.StudentId,
        TeacherId = user.TeacherId
    };
}
