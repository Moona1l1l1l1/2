using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.DTO.Courses.V2;

namespace UniversityApi.Services.Interfaces;

public interface ICourseService
{
    Task<ReturnResult<List<CourseDto>>> GetAllAsync(CourseFilterDto filter, CancellationToken ct = default);
    Task<ReturnResult<CourseDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ReturnResult<CourseDto>> CreateAsync(CourseCreateDto dto, CancellationToken ct = default);
    Task<ReturnResult<CourseDto>> UpdateAsync(int id, CourseUpdateDto dto, CancellationToken ct = default);
    Task<ReturnResult<List<CourseV2Dto>>> GetAllV2Async(CourseFilterDto filter, CancellationToken ct = default);
    Task<ReturnResult<CourseV2Dto>> GetByIdV2Async(int id, CancellationToken ct = default);
    Task<ReturnResult<int>> DeleteAsync(int id, CancellationToken ct = default);
}
