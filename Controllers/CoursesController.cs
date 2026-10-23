using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiVersion("1.0")]
[Route("api/courses")]                       // прежний маршрут из задания 1 продолжает работать
[Route("api/v{version:apiVersion}/courses")]
public class CoursesController : BaseApiController
{
    private readonly ICourseService _service;

    public CoursesController(ICourseService service)
    {
        _service = service;
    }

    /// <summary>Получить курсы. Фильтры: name, teacherId, minCredits, maxCredits.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReturnResult<List<CourseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<List<CourseDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] CourseFilterDto filter, CancellationToken ct)
        => ToActionResult(await _service.GetAllAsync(filter, ct));

    /// <summary>Получить курс по Id.</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => ToActionResult(await _service.GetByIdAsync(id, ct));

    /// <summary>Создать курс и закрепить его за преподавателем.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CourseCreateDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        result.TraceId = HttpContext.TraceIdentifier;
        return CreatedAtAction(nameof(GetById), new { id = result.Result!.Id }, result);
    }

    /// <summary>Изменить курс.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CourseUpdateDto dto, CancellationToken ct)
        => ToActionResult(await _service.UpdateAsync(id, dto, ct));

    /// <summary>Удалить курс (нельзя, если на курс записаны студенты).</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<int>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<int>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => ToActionResult(await _service.DeleteAsync(id, ct));
}
