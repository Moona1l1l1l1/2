using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Teachers;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class TeacherService : ITeacherService
{
    private readonly ITeacherRepository _teachers;
    private readonly IMapper _mapper;
    private readonly ILogger<TeacherService> _logger;

    public TeacherService(ITeacherRepository teachers, IMapper mapper, ILogger<TeacherService> logger)
    {
        _teachers = teachers;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReturnResult<List<TeacherDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var teachers = await _teachers.GetAllAsync(ct);
        return ReturnResult<List<TeacherDto>>.Ok(_mapper.Map<List<TeacherDto>>(teachers));
    }

    public async Task<ReturnResult<TeacherDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var teacher = await _teachers.GetByIdAsync(id, ct);
        if (teacher is null)
            return NotFound<TeacherDto>(id);

        return ReturnResult<TeacherDto>.Ok(_mapper.Map<TeacherDto>(teacher));
    }

    public async Task<ReturnResult<TeacherDto>> CreateAsync(TeacherCreateDto dto, CancellationToken ct = default)
    {
        if (await _teachers.EmailExistsAsync(dto.Email, null, ct))
        {
            _logger.LogWarning("Teacher creation rejected: email {Email} already exists", dto.Email);
            return ReturnResult<TeacherDto>.Fail(409, ErrorCodes.TeacherEmailExists,
                "Teacher with this email already exists");
        }

        var teacher = _mapper.Map<Teacher>(dto);
        teacher.Email = teacher.Email.Trim();
        await _teachers.AddAsync(teacher, ct);

        _logger.LogInformation("Teacher {TeacherId} created", teacher.Id);
        return ReturnResult<TeacherDto>.Created(_mapper.Map<TeacherDto>(teacher));
    }

    public async Task<ReturnResult<TeacherDto>> UpdateAsync(int id, TeacherUpdateDto dto, CancellationToken ct = default)
    {
        var teacher = await _teachers.GetByIdAsync(id, ct);
        if (teacher is null)
            return NotFound<TeacherDto>(id);

        if (await _teachers.EmailExistsAsync(dto.Email, id, ct))
        {
            _logger.LogWarning("Teacher {TeacherId} update rejected: email {Email} already exists", id, dto.Email);
            return ReturnResult<TeacherDto>.Fail(409, ErrorCodes.TeacherEmailExists,
                "Teacher with this email already exists");
        }

        _mapper.Map(dto, teacher);
        teacher.Email = teacher.Email.Trim();
        await _teachers.UpdateAsync(teacher, ct);

        _logger.LogInformation("Teacher {TeacherId} updated", id);
        return ReturnResult<TeacherDto>.Ok(_mapper.Map<TeacherDto>(teacher));
    }

    public async Task<ReturnResult<int>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var teacher = await _teachers.GetByIdAsync(id, ct);
        if (teacher is null)
            return NotFound<int>(id);

        if (await _teachers.HasCoursesAsync(id, ct))
        {
            _logger.LogWarning("Teacher {TeacherId} deletion rejected: teacher still has courses", id);
            return ReturnResult<int>.Fail(409, ErrorCodes.TeacherHasCourses,
                "Teacher cannot be deleted while assigned to courses");
        }

        await _teachers.DeleteAsync(teacher, ct);

        _logger.LogInformation("Teacher {TeacherId} deleted", id);
        return ReturnResult<int>.Ok(id);
    }

    private ReturnResult<T> NotFound<T>(int id)
    {
        _logger.LogWarning("Teacher {TeacherId} not found", id);
        return ReturnResult<T>.Fail(404, ErrorCodes.TeacherNotFound, "Teacher not found");
    }
}
