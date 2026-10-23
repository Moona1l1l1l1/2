using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Auth;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiVersionNeutral]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    /// <summary>Регистрация. Новый пользователь получает только роль Student (без прав администратора).</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        => ToActionResult(await _service.RegisterAsync(dto, ct));

    /// <summary>Вход. При успехе возвращает JWT Access Token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReturnResult<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        => ToActionResult(await _service.LoginAsync(dto, ct));

    /// <summary>Информация о текущем пользователе (по JWT).</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = User.FindFirstValue(AppClaims.Sub);
        if (userId is null)
            return ToActionResult(ReturnResult<object>.Fail(401, ErrorCodes.Unauthorized, "Authentication is required"));

        return ToActionResult(await _service.GetCurrentAsync(userId, ct));
    }
}
