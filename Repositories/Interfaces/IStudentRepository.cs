using UniversityApi.Models;

namespace UniversityApi.Repositories.Interfaces;

public interface IStudentRepository : IRepository<Student>
{
    /// <summary>Студент вместе с записями на курсы и самими курсами.</summary>
    Task<Student?> GetWithCoursesAsync(int id, CancellationToken ct = default);

    Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken ct = default);
}
