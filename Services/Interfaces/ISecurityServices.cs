using UniversityApi.Common;
using UniversityApi.DTO.Auth;
using UniversityApi.Models;

namespace UniversityApi.Services.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(ApplicationUser user, IEnumerable<string> roles);
}

public interface ICurrentUser
{
    string? UserId { get; }
    int? StudentId { get; }
    int? TeacherId { get; }
    bool IsAdmin { get; }
    bool IsTeacher { get; }
    bool IsStudent { get; }
}

/// <summary>Проверки принадлежности ресурсов (роли недостаточно: нужно ещё владение данными).</summary>
public interface IResourceAccessService
{
    /// <summary>Admin — любой студент; Student — только себя; Teacher — студентов своих курсов.</summary>
    Task<bool> CanViewStudentAsync(int studentId, CancellationToken ct = default);

    /// <summary>Admin — любой курс; Teacher — только собственный.</summary>
    Task<bool> CanManageCourseGradesAsync(int courseId, CancellationToken ct = default);

    /// <summary>Admin — любая запись; Student — своя; Teacher — запись на собственном курсе.</summary>
    Task<bool> CanViewEnrollmentAsync(int studentId, int courseId, CancellationToken ct = default);

    Task<List<int>> GetOwnedCourseIdsAsync(CancellationToken ct = default);
}

public interface IAuthService
{
    Task<ReturnResult<UserDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<ReturnResult<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<ReturnResult<UserDto>> GetCurrentAsync(string userId, CancellationToken ct = default);
}

public interface IUserAdminService
{
    Task<ReturnResult<List<UserDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ReturnResult<UserDto>> AssignRoleAsync(string userId, string role, CancellationToken ct = default);
    Task<ReturnResult<UserDto>> RemoveRoleAsync(string userId, string role, string? actingUserId, CancellationToken ct = default);
    Task<ReturnResult<UserDto>> LinkAsync(string userId, DTO.Auth.LinkUserDto dto, CancellationToken ct = default);
}
