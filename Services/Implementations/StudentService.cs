using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Students;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _students;
    private readonly IMapper _mapper;
    private readonly ILogger<StudentService> _logger;

    public StudentService(IStudentRepository students, IMapper mapper, ILogger<StudentService> logger)
    {
        _students = students;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReturnResult<List<StudentDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var students = await _students.GetAllAsync(ct);
        return ReturnResult<List<StudentDto>>.Ok(_mapper.Map<List<StudentDto>>(students));
    }

    public async Task<ReturnResult<StudentDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var student = await _students.GetByIdAsync(id, ct);
        if (student is null)
            return NotFound<StudentDto>(id);

        return ReturnResult<StudentDto>.Ok(_mapper.Map<StudentDto>(student));
    }

    public async Task<ReturnResult<StudentDetailsDto>> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        var student = await _students.GetWithCoursesAsync(id, ct);
        if (student is null)
            return NotFound<StudentDetailsDto>(id);

        return ReturnResult<StudentDetailsDto>.Ok(_mapper.Map<StudentDetailsDto>(student));
    }

    public async Task<ReturnResult<StudentDto>> CreateAsync(StudentCreateDto dto, CancellationToken ct = default)
    {
        if (await _students.EmailExistsAsync(dto.Email, null, ct))
        {
            _logger.LogWarning("Student creation rejected: email {Email} already exists", dto.Email);
            return ReturnResult<StudentDto>.Fail(409, ErrorCodes.StudentEmailExists,
                "Student with this email already exists");
        }

        var student = _mapper.Map<Student>(dto);
        student.Email = student.Email.Trim();
        await _students.AddAsync(student, ct);

        _logger.LogInformation("Student {StudentId} created", student.Id);
        return ReturnResult<StudentDto>.Created(_mapper.Map<StudentDto>(student));
    }

    public async Task<ReturnResult<StudentDto>> UpdateAsync(int id, StudentUpdateDto dto, CancellationToken ct = default)
    {
        var student = await _students.GetByIdAsync(id, ct);
        if (student is null)
            return NotFound<StudentDto>(id);

        if (await _students.EmailExistsAsync(dto.Email, id, ct))
        {
            _logger.LogWarning("Student {StudentId} update rejected: email {Email} already exists", id, dto.Email);
            return ReturnResult<StudentDto>.Fail(409, ErrorCodes.StudentEmailExists,
                "Student with this email already exists");
        }

        _mapper.Map(dto, student);
        student.Email = student.Email.Trim();
        await _students.UpdateAsync(student, ct);

        _logger.LogInformation("Student {StudentId} updated", id);
        return ReturnResult<StudentDto>.Ok(_mapper.Map<StudentDto>(student));
    }

    public async Task<ReturnResult<int>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var student = await _students.GetByIdAsync(id, ct);
        if (student is null)
            return NotFound<int>(id);

        await _students.DeleteAsync(student, ct);

        _logger.LogInformation("Student {StudentId} deleted", id);
        return ReturnResult<int>.Ok(id);
    }

    private ReturnResult<T> NotFound<T>(int id)
    {
        _logger.LogWarning("Student {StudentId} not found", id);
        return ReturnResult<T>.Fail(404, ErrorCodes.StudentNotFound, "Student not found");
    }
}
