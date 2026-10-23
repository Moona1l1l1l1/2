using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Enrollments;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[Route("api/enrollments")]
public class EnrollmentsController : BaseApiController
{
    private readonly IEnrollmentService _service;
    private readonly IResourceAccessService _access;
    private readonly ICurrentUser _user;
    private readonly ILogger<EnrollmentsController> _logger;

    public EnrollmentsController(IEnrollmentService service, IResourceAccessService access, ICurrentUser user,
        ILogger<EnrollmentsController> logger)
    {
        _service = service;
        _access = access;
        _user = user;
        _logger = logger;
    }

    /// <summary>
    /// Получить список записей. Admin — все; Teacher — только по своим курсам; Student — только свои.
    /// Необязательные фильтры: studentId, courseId.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<List<EnrollmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] int? studentId, [FromQuery] int? courseId, CancellationToken ct)
    {
        if (_user.IsAdmin)
            return ToActionResult(await _service.GetAllAsync(studentId, courseId, ct));

        if (_user.IsTeacher)
        {
            if (courseId.HasValue && !await _access.CanManageCourseGradesAsync(courseId.Value, ct))
                return ForbiddenResult();

            var owned = await _access.GetOwnedCourseIdsAsync(ct);
            var result = await _service.GetAllAsync(studentId, courseId, ct);
            if (result.IsSuccess)
                result.Result = result.Result!.Where(e => owned.Contains(e.CourseId)).ToList();
            return ToActionResult(result);
        }

        // Student: принудительно только свои записи
        if (_user.StudentId is not int ownStudentId)
            return ToActionResult(ReturnResult<List<EnrollmentDto>>.Ok(new List<EnrollmentDto>()));

        if (studentId.HasValue && studentId.Value != ownStudentId)
        {
            _logger.LogWarning("User {UserId} tried to read enrollments of student {StudentId}", _user.UserId, studentId);
            return ForbiddenResult();
        }

        return ToActionResult(await _service.GetAllAsync(ownStudentId, courseId, ct));
    }

    /// <summary>Получить запись по Id (с проверкой принадлежности).</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        if (!await _access.CanViewEnrollmentAsync(result.Result!.StudentId, result.Result.CourseId, ct))
            return ForbiddenResult();

        return ToActionResult(result);
    }

    /// <summary>Записать студента на курс (только Admin).</summary>
    [HttpPost]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enroll([FromBody] EnrollmentCreateDto dto, CancellationToken ct)
    {
        var result = await _service.EnrollAsync(dto, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        result.TraceId = HttpContext.TraceIdentifier;
        return CreatedAtAction(nameof(GetById), new { id = result.Result!.Id }, result);
    }

    /// <summary>Изменить оценку. Admin — любую; Teacher — только на собственном курсе.</summary>
    [HttpPut("{id}/grade")]
    [Authorize(Policy = Policies.TeacherOrAdmin)]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGrade(int id, [FromBody] EnrollmentUpdateGradeDto dto, CancellationToken ct)
    {
        var existing = await _service.GetByIdAsync(id, ct);
        if (!existing.IsSuccess)
            return ToActionResult(existing);

        // Одной проверки роли мало: преподаватель должен вести именно этот курс
        if (!await _access.CanManageCourseGradesAsync(existing.Result!.CourseId, ct))
        {
            _logger.LogWarning("User {UserId} tried to change a grade on foreign course {CourseId}",
                _user.UserId, existing.Result.CourseId);
            return ForbiddenResult();
        }

        return ToActionResult(await _service.UpdateGradeAsync(id, dto, ct));
    }

    /// <summary>Удалить студента с курса (только Admin).</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<int>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => ToActionResult(await _service.DeleteAsync(id, ct));
}
