using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;

namespace UniversityApi.Controllers;

/// <summary>
/// Безопасность «по умолчанию»: все наследники требуют аутентификации.
/// Публичные endpoints помечаются явно через [AllowAnonymous].
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>Преобразует ReturnResult в HTTP-ответ: StatusCode результата становится HTTP-кодом.</summary>
    protected IActionResult ToActionResult<T>(ReturnResult<T> result)
    {
        result.TraceId = HttpContext.TraceIdentifier;
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>403: пользователь аутентифицирован, но ресурс ему не принадлежит / нет прав.</summary>
    protected IActionResult ForbiddenResult()
        => ToActionResult(ReturnResult<object>.Fail(StatusCodes.Status403Forbidden, ErrorCodes.Forbidden,
            "You do not have permission to access this resource"));
}
