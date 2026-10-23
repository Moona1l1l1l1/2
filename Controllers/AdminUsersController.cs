using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Auth;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

/// <summary>Управление пользователями и ролями. Доступно только роли Admin.</summary>
[ApiVersionNeutral]
[Route("api/admin/users")]
[Authorize(Policy = Policies.AdminOnly)]
public class AdminUsersController : BaseApiController
{
    private readonly IUserAdminService _service;
    private readonly ICurrentUser _user;

    public AdminUsersController(IUserAdminService service, ICurrentUser user)
    {
        _service = service;
        _user = user;
    }

    /// <summary>Список пользователей (без хешей паролей).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<List<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => ToActionResult(await _service.GetAllAsync(ct));

    /// <summary>Назначить роль (Admin, Teacher, Student).</summary>
    [HttpPost("{userId}/roles")]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleDto dto, CancellationToken ct)
        => ToActionResult(await _service.AssignRoleAsync(userId, dto.Role, ct));

    /// <summary>Снять роль.</summary>
    [HttpDelete("{userId}/roles/{role}")]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveRole(string userId, string role, CancellationToken ct)
        => ToActionResult(await _service.RemoveRoleAsync(userId, role, _user.UserId, ct));

    /// <summary>Связать учётную запись с сущностью Student и/или Teacher.</summary>
    [HttpPut("{userId}/link")]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Link(string userId, [FromBody] LinkUserDto dto, CancellationToken ct)
        => ToActionResult(await _service.LinkAsync(userId, dto, ct));
}
