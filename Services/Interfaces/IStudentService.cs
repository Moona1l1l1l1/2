using UniversityApi.Common;
using UniversityApi.DTO.Students;

namespace UniversityApi.Services.Interfaces;

public interface IStudentService
{
    Task<ReturnResult<List<StudentDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ReturnResult<StudentDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ReturnResult<StudentDetailsDto>> GetDetailsAsync(int id, CancellationToken ct = default);
    Task<ReturnResult<StudentDto>> CreateAsync(StudentCreateDto dto, CancellationToken ct = default);
    Task<ReturnResult<StudentDto>> UpdateAsync(int id, StudentUpdateDto dto, CancellationToken ct = default);
    Task<ReturnResult<int>> DeleteAsync(int id, CancellationToken ct = default);
}
