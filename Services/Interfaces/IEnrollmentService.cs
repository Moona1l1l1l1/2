using UniversityApi.Common;
using UniversityApi.DTO.Enrollments;

namespace UniversityApi.Services.Interfaces;

public interface IEnrollmentService
{
    Task<ReturnResult<List<EnrollmentDto>>> GetAllAsync(int? studentId, int? courseId, CancellationToken ct = default);
    Task<ReturnResult<EnrollmentDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ReturnResult<EnrollmentDto>> EnrollAsync(EnrollmentCreateDto dto, CancellationToken ct = default);
    Task<ReturnResult<EnrollmentDto>> UpdateGradeAsync(int id, EnrollmentUpdateGradeDto dto, CancellationToken ct = default);
    Task<ReturnResult<int>> DeleteAsync(int id, CancellationToken ct = default);
}
