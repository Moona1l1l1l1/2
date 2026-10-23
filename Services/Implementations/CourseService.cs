using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.DTO.Courses.V2;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courses;
    private readonly ITeacherRepository _teachers;
    private readonly IMapper _mapper;
    private readonly ILogger<CourseService> _logger;

    public CourseService(ICourseRepository courses, ITeacherRepository teachers, IMapper mapper,
        ILogger<CourseService> logger)
    {
        _courses = courses;
        _teachers = teachers;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReturnResult<List<CourseDto>>> GetAllAsync(CourseFilterDto filter, CancellationToken ct = default)
    {
        if (IsFilterInvalid(filter))
            return InvalidFilter<List<CourseDto>>(filter);

        var courses = await _courses.GetFilteredAsync(filter, ct);
        return ReturnResult<List<CourseDto>>.Ok(_mapper.Map<List<CourseDto>>(courses));
    }

    public async Task<ReturnResult<CourseDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var course = await _courses.GetByIdAsync(id, ct);
        if (course is null)
            return NotFound<CourseDto>(id);

        return ReturnResult<CourseDto>.Ok(_mapper.Map<CourseDto>(course));
    }

    public async Task<ReturnResult<CourseDto>> CreateAsync(CourseCreateDto dto, CancellationToken ct = default)
    {
        // Правило №4: курс не может ссылаться на несуществующего преподавателя
        var teacher = await _teachers.GetByIdAsync(dto.TeacherId, ct);
        if (teacher is null)
            return TeacherNotFound<CourseDto>(dto.TeacherId);

        var course = _mapper.Map<Course>(dto);
        course.Teacher = teacher;
        await _courses.AddAsync(course, ct);

        _logger.LogInformation("Course {CourseId} created for teacher {TeacherId}", course.Id, teacher.Id);
        return ReturnResult<CourseDto>.Created(_mapper.Map<CourseDto>(course));
    }

    public async Task<ReturnResult<CourseDto>> UpdateAsync(int id, CourseUpdateDto dto, CancellationToken ct = default)
    {
        var course = await _courses.GetByIdAsync(id, ct);
        if (course is null)
            return NotFound<CourseDto>(id);

        var teacher = await _teachers.GetByIdAsync(dto.TeacherId, ct);
        if (teacher is null)
            return TeacherNotFound<CourseDto>(dto.TeacherId);

        _mapper.Map(dto, course);
        course.Teacher = teacher;
        await _courses.UpdateAsync(course, ct);

        _logger.LogInformation("Course {CourseId} updated", id);
        return ReturnResult<CourseDto>.Ok(_mapper.Map<CourseDto>(course));
    }

    public async Task<ReturnResult<int>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var course = await _courses.GetByIdAsync(id, ct);
        if (course is null)
            return NotFound<int>(id);

        if (await _courses.HasEnrollmentsAsync(id, ct))
        {
            _logger.LogWarning("Course {CourseId} deletion rejected: course has enrollments", id);
            return ReturnResult<int>.Fail(409, ErrorCodes.CourseHasEnrollments,
                "Course cannot be deleted while students are enrolled");
        }

        await _courses.DeleteAsync(course, ct);

        _logger.LogInformation("Course {CourseId} deleted", id);
        return ReturnResult<int>.Ok(id);
    }


    // ---- Версия 2: та же бизнес-логика (фильтры, поиск), другой контракт ответа ----

    public async Task<ReturnResult<List<CourseV2Dto>>> GetAllV2Async(CourseFilterDto filter, CancellationToken ct = default)
    {
        if (IsFilterInvalid(filter))
            return InvalidFilter<List<CourseV2Dto>>(filter);

        var courses = await _courses.GetFilteredAsync(filter, ct);
        var counts = await _courses.GetEnrollmentCountsAsync(courses.Select(c => c.Id).ToList(), ct);

        var dtos = _mapper.Map<List<CourseV2Dto>>(courses);
        foreach (var dto in dtos)
            dto.EnrolledStudentsCount = counts.GetValueOrDefault(dto.Id);

        return ReturnResult<List<CourseV2Dto>>.Ok(dtos);
    }

    public async Task<ReturnResult<CourseV2Dto>> GetByIdV2Async(int id, CancellationToken ct = default)
    {
        var course = await _courses.GetByIdAsync(id, ct);
        if (course is null)
            return NotFound<CourseV2Dto>(id);

        var counts = await _courses.GetEnrollmentCountsAsync(new List<int> { id }, ct);

        var dto = _mapper.Map<CourseV2Dto>(course);
        dto.EnrolledStudentsCount = counts.GetValueOrDefault(id);
        return ReturnResult<CourseV2Dto>.Ok(dto);
    }

    private static bool IsFilterInvalid(CourseFilterDto filter)
        => filter.MinCredits.HasValue && filter.MaxCredits.HasValue && filter.MinCredits > filter.MaxCredits;

    private ReturnResult<T> InvalidFilter<T>(CourseFilterDto filter)
    {
        _logger.LogWarning("Invalid course filter: MinCredits {Min} is greater than MaxCredits {Max}",
            filter.MinCredits, filter.MaxCredits);
        return ReturnResult<T>.Fail(400, ErrorCodes.InvalidFilter, "MinCredits cannot be greater than MaxCredits");
    }

    private ReturnResult<T> NotFound<T>(int id)
    {
        _logger.LogWarning("Course {CourseId} not found", id);
        return ReturnResult<T>.Fail(404, ErrorCodes.CourseNotFound, "Course not found");
    }

    private ReturnResult<T> TeacherNotFound<T>(int teacherId)
    {
        _logger.LogWarning("Teacher {TeacherId} not found while saving a course", teacherId);
        return ReturnResult<T>.Fail(404, ErrorCodes.TeacherNotFound, "Teacher not found");
    }
}
