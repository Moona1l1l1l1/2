using UniversityApi.Common;
using UniversityApi.DTO.Teachers;

namespace UniversityApi.Services.Interfaces;

public interface ITeacherService
{
    Task<ReturnResult<List<TeacherDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ReturnResult<TeacherDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ReturnResult<TeacherDto>> CreateAsync(TeacherCreateDto dto, CancellationToken ct = default);
    Task<ReturnResult<TeacherDto>> UpdateAsync(int id, TeacherUpdateDto dto, CancellationToken ct = default);
    Task<ReturnResult<int>> DeleteAsync(int id, CancellationToken ct = default);
}
