using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Teachers;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[Route("api/teachers")]
public class TeachersController : BaseApiController
{
    private readonly ITeacherService _service;

    public TeachersController(ITeacherService service)
    {
        _service = service;
    }

    /// <summary>Получить всех преподавателей.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<List<TeacherDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => ToActionResult(await _service.GetAllAsync(ct));

    /// <summary>Получить преподавателя по Id.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => ToActionResult(await _service.GetByIdAsync(id, ct));

    /// <summary>Создать преподавателя.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] TeacherCreateDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        result.TraceId = HttpContext.TraceIdentifier;
        return CreatedAtAction(nameof(GetById), new { id = result.Result!.Id }, result);
    }

    /// <summary>Изменить преподавателя.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] TeacherUpdateDto dto, CancellationToken ct)
        => ToActionResult(await _service.UpdateAsync(id, dto, ct));

    /// <summary>Удалить преподавателя (нельзя, если за ним закреплены курсы).</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<int>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<int>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => ToActionResult(await _service.DeleteAsync(id, ct));
}
