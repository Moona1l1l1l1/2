using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class StudentRepository : Repository<Student>, IStudentRepository
{
    public StudentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Student?> GetWithCoursesAsync(int id, CancellationToken ct = default)
        => await Set
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Course)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLower();
        return await Set.AnyAsync(
            s => s.Email.ToLower() == normalized && (excludeId == null || s.Id != excludeId), ct);
    }
}
