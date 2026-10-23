using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Enrollments;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollments;
    private readonly IStudentRepository _students;
    private readonly ICourseRepository _courses;
    private readonly IMapper _mapper;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(IEnrollmentRepository enrollments, IStudentRepository students,
        ICourseRepository courses, IMapper mapper, ILogger<EnrollmentService> logger)
    {
        _enrollments = enrollments;
        _students = students;
        _courses = courses;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReturnResult<List<EnrollmentDto>>> GetAllAsync(int? studentId, int? courseId,
        CancellationToken ct = default)
    {
        var items = await _enrollments.GetFilteredAsync(studentId, courseId, ct);
        return ReturnResult<List<EnrollmentDto>>.Ok(_mapper.Map<List<EnrollmentDto>>(items));
    }

    public async Task<ReturnResult<EnrollmentDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var enrollment = await _enrollments.GetByIdAsync(id, ct);
        if (enrollment is null)
            return NotFound<EnrollmentDto>(id);

        return ReturnResult<EnrollmentDto>.Ok(_mapper.Map<EnrollmentDto>(enrollment));
    }

    public async Task<ReturnResult<EnrollmentDto>> EnrollAsync(EnrollmentCreateDto dto, CancellationToken ct = default)
    {
        // Правило №2: несуществующий студент
        var student = await _students.GetByIdAsync(dto.StudentId, ct);
        if (student is null)
        {
            _logger.LogWarning("Enrollment rejected: student {StudentId} not found", dto.StudentId);
            return ReturnResult<EnrollmentDto>.Fail(404, ErrorCodes.StudentNotFound, "Student not found");
        }

        // Правило №1: несуществующий курс
        var course = await _courses.GetByIdAsync(dto.CourseId, ct);
        if (course is null)
        {
            _logger.LogWarning("Enrollment rejected: course {CourseId} not found", dto.CourseId);
            return ReturnResult<EnrollmentDto>.Fail(404, ErrorCodes.CourseNotFound, "Course not found");
        }

        // Правило №3: повторная запись
        if (await _enrollments.ExistsForStudentAndCourseAsync(dto.StudentId, dto.CourseId, ct))
        {
            _logger.LogWarning("Enrollment rejected: student {StudentId} is already enrolled in course {CourseId}",
                dto.StudentId, dto.CourseId);
            return ReturnResult<EnrollmentDto>.Fail(409, ErrorCodes.EnrollmentAlreadyExists,
                "Student is already enrolled in this course");
        }

        var enrollment = _mapper.Map<Enrollment>(dto);
        enrollment.Student = student;
        enrollment.Course = course;
        await _enrollments.AddAsync(enrollment, ct);

        _logger.LogInformation("Student {StudentId} enrolled in course {CourseId} (enrollment {EnrollmentId})",
            dto.StudentId, dto.CourseId, enrollment.Id);
        return ReturnResult<EnrollmentDto>.Created(_mapper.Map<EnrollmentDto>(enrollment));
    }

    public async Task<ReturnResult<EnrollmentDto>> UpdateGradeAsync(int id, EnrollmentUpdateGradeDto dto,
        CancellationToken ct = default)
    {
        var enrollment = await _enrollments.GetByIdAsync(id, ct);
        if (enrollment is null)
            return NotFound<EnrollmentDto>(id);

        _mapper.Map(dto, enrollment);
        await _enrollments.UpdateAsync(enrollment, ct);

        _logger.LogInformation("Grade of enrollment {EnrollmentId} changed to {Grade}", id, enrollment.Grade);
        return ReturnResult<EnrollmentDto>.Ok(_mapper.Map<EnrollmentDto>(enrollment));
    }

    public async Task<ReturnResult<int>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var enrollment = await _enrollments.GetByIdAsync(id, ct);
        if (enrollment is null)
            return NotFound<int>(id);

        await _enrollments.DeleteAsync(enrollment, ct);

        _logger.LogInformation("Enrollment {EnrollmentId} deleted", id);
        return ReturnResult<int>.Ok(id);
    }

    private ReturnResult<T> NotFound<T>(int id)
    {
        _logger.LogWarning("Enrollment {EnrollmentId} not found", id);
        return ReturnResult<T>.Fail(404, ErrorCodes.EnrollmentNotFound, "Enrollment not found");
    }
}
