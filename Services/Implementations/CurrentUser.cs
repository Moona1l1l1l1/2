using UniversityApi.Common;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

/// <summary>Данные текущего пользователя из проверенного (подписанного) JWT.</summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private System.Security.Claims.ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public string? UserId => Principal?.FindFirst(AppClaims.Sub)?.Value;
    public int? StudentId => ParseInt(AppClaims.StudentId);
    public int? TeacherId => ParseInt(AppClaims.TeacherId);
    public bool IsAdmin => Principal?.IsInRole(Roles.Admin) ?? false;
    public bool IsTeacher => Principal?.IsInRole(Roles.Teacher) ?? false;
    public bool IsStudent => Principal?.IsInRole(Roles.Student) ?? false;

    private int? ParseInt(string claimType)
        => int.TryParse(Principal?.FindFirst(claimType)?.Value, out var value) ? value : null;
}
