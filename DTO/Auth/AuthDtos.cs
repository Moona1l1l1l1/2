using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Auth;

public class RegisterDto
{
    [Required, StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAtUtc { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

/// <summary>Публичное представление пользователя. Без PasswordHash, SecurityStamp и других служебных полей.</summary>
public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = new();
    public int? StudentId { get; set; }
    public int? TeacherId { get; set; }
}

public class AssignRoleDto
{
    [Required]
    public string Role { get; set; } = string.Empty;
}

public class LinkUserDto
{
    /// <summary>Id студента (null — снять связь).</summary>
    public int? StudentId { get; set; }

    /// <summary>Id преподавателя (null — снять связь).</summary>
    public int? TeacherId { get; set; }
}
