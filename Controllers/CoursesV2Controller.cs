using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.DTO.Courses.V2;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

/// <summary>Версия 2 чтения курсов. Использует тот же ICourseService, что и v1 (без дублирования логики).</summary>
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/courses")]
public class CoursesV2Controller : BaseApiController
{
    private readonly ICourseService _service;

    public CoursesV2Controller(ICourseService service)
    {
        _service = service;
    }

    /// <summary>Получить курсы (v2: с преподавателем и числом записанных студентов).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReturnResult<List<CourseV2Dto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<List<CourseV2Dto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] CourseFilterDto filter, CancellationToken ct)
        => ToActionResult(await _service.GetAllV2Async(filter, ct));

    /// <summary>Получить курс по Id (v2).</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReturnResult<CourseV2Dto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<CourseV2Dto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => ToActionResult(await _service.GetByIdV2Async(id, ct));
}
