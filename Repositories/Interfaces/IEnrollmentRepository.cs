using UniversityApi.Models;

namespace UniversityApi.Repositories.Interfaces;

public interface IEnrollmentRepository : IRepository<Enrollment>
{
    Task<IReadOnlyList<Enrollment>> GetFilteredAsync(int? studentId, int? courseId, CancellationToken ct = default);
    Task<bool> ExistsForStudentAndCourseAsync(int studentId, int courseId, CancellationToken ct = default);
}
