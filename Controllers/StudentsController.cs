using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Students;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[Route("api/students")]
public class StudentsController : BaseApiController
{
    private readonly IStudentService _service;

    private readonly IResourceAccessService _access;

    public StudentsController(IStudentService service, IResourceAccessService access)
    {
        _service = service;
        _access = access;
    }

    /// <summary>Получить всех студентов.</summary>
    [HttpGet]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<List<StudentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => ToActionResult(await _service.GetAllAsync(ct));

    /// <summary>Получить студента по Id.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        if (!await _access.CanViewStudentAsync(id, ct))
            return ForbiddenResult();

        return ToActionResult(await _service.GetByIdAsync(id, ct));
    }

    /// <summary>Получить студента вместе с его курсами и оценками.</summary>
    [HttpGet("{id}/details")]
    [ProducesResponseType(typeof(ReturnResult<StudentDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<StudentDetailsDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetails(int id, CancellationToken ct)
    {
        // Оценки — чувствительные данные: студент видит только свои, преподаватель — студентов своих курсов
        if (!await _access.CanViewStudentAsync(id, ct))
            return ForbiddenResult();

        return ToActionResult(await _service.GetDetailsAsync(id, ct));
    }

    /// <summary>Создать студента.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] StudentCreateDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        if (!result.IsSuccess)
            return ToActionResult(result);

        result.TraceId = HttpContext.TraceIdentifier;
        return CreatedAtAction(nameof(GetById), new { id = result.Result!.Id }, result);
    }

    /// <summary>Изменить студента.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] StudentUpdateDto dto, CancellationToken ct)
        => ToActionResult(await _service.UpdateAsync(id, dto, ct));

    /// <summary>Удалить студента (его записи на курсы удаляются каскадно).</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<int>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => ToActionResult(await _service.DeleteAsync(id, ct));
}
