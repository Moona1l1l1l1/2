using UniversityApi.DTO.Courses;
using UniversityApi.Models;

namespace UniversityApi.Repositories.Interfaces;

public interface ICourseRepository : IRepository<Course>
{
    Task<IReadOnlyList<Course>> GetFilteredAsync(CourseFilterDto filter, CancellationToken ct = default);
    Task<Dictionary<int, int>> GetEnrollmentCountsAsync(IReadOnlyCollection<int> courseIds, CancellationToken ct = default);
    Task<bool> HasEnrollmentsAsync(int courseId, CancellationToken ct = default);
}
