using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniversityApi.Common;
using UniversityApi.DTO.Auth;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class UserAdminService : IUserAdminService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IStudentRepository _students;
    private readonly ITeacherRepository _teachers;
    private readonly ILogger<UserAdminService> _logger;

    public UserAdminService(UserManager<ApplicationUser> users, IStudentRepository students,
        ITeacherRepository teachers, ILogger<UserAdminService> logger)
    {
        _users = users;
        _students = students;
        _teachers = teachers;
        _logger = logger;
    }

    public async Task<ReturnResult<List<UserDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _users.Users.AsNoTracking().OrderBy(u => u.UserName).ToListAsync(ct);
        var list = new List<UserDto>();
        foreach (var user in users)
            list.Add(await ToDtoAsync(user));

        return ReturnResult<List<UserDto>>.Ok(list);
    }

    public async Task<ReturnResult<UserDto>> AssignRoleAsync(string userId, string role, CancellationToken ct = default)
    {
        var canonical = Roles.All.FirstOrDefault(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
        if (canonical is null)
            return ReturnResult<UserDto>.Fail(400, ErrorCodes.RoleInvalid, "Role must be one of: Admin, Teacher, Student");

        var user = await _users.FindByIdAsync(userId);
        if (user is null)
            return ReturnResult<UserDto>.Fail(404, ErrorCodes.UserNotFound, "User not found");

        if (!await _users.IsInRoleAsync(user, canonical))
        {
            var result = await _users.AddToRoleAsync(user, canonical);
            if (!result.Succeeded)
                return ReturnResult<UserDto>.Fail(400, ErrorCodes.RoleInvalid, "Role could not be assigned");
        }

        _logger.LogInformation("Role {Role} assigned to user {UserId}", canonical, user.Id);
        return ReturnResult<UserDto>.Ok(await ToDtoAsync(user));
    }

    public async Task<ReturnResult<UserDto>> RemoveRoleAsync(string userId, string role, string? actingUserId,
        CancellationToken ct = default)
    {
        var canonical = Roles.All.FirstOrDefault(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
        if (canonical is null)
            return ReturnResult<UserDto>.Fail(400, ErrorCodes.RoleInvalid, "Role must be one of: Admin, Teacher, Student");

        var user = await _users.FindByIdAsync(userId);
        if (user is null)
            return ReturnResult<UserDto>.Fail(404, ErrorCodes.UserNotFound, "User not found");

        if (canonical == Roles.Admin && user.Id == actingUserId)
            return ReturnResult<UserDto>.Fail(409, ErrorCodes.OperationNotAllowed,
                "Administrators cannot remove their own Admin role");

        if (await _users.IsInRoleAsync(user, canonical))
            await _users.RemoveFromRoleAsync(user, canonical);

        _logger.LogInformation("Role {Role} removed from user {UserId}", canonical, user.Id);
        return ReturnResult<UserDto>.Ok(await ToDtoAsync(user));
    }

    public async Task<ReturnResult<UserDto>> LinkAsync(string userId, LinkUserDto dto, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null)
            return ReturnResult<UserDto>.Fail(404, ErrorCodes.UserNotFound, "User not found");

        if (dto.StudentId.HasValue && await _students.GetByIdAsync(dto.StudentId.Value, ct) is null)
            return ReturnResult<UserDto>.Fail(404, ErrorCodes.StudentNotFound, "Student not found");

        if (dto.TeacherId.HasValue && await _teachers.GetByIdAsync(dto.TeacherId.Value, ct) is null)
            return ReturnResult<UserDto>.Fail(404, ErrorCodes.TeacherNotFound, "Teacher not found");

        user.StudentId = dto.StudentId;
        user.TeacherId = dto.TeacherId;
        await _users.UpdateAsync(user);

        _logger.LogInformation("User {UserId} linked to student {StudentId} / teacher {TeacherId}",
            user.Id, dto.StudentId, dto.TeacherId);
        return ReturnResult<UserDto>.Ok(await ToDtoAsync(user));
    }

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
