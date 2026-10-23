using UniversityApi.Models;

namespace UniversityApi.Repositories.Interfaces;

public interface ITeacherRepository : IRepository<Teacher>
{
    Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken ct = default);
    Task<bool> HasCoursesAsync(int teacherId, CancellationToken ct = default);
}
